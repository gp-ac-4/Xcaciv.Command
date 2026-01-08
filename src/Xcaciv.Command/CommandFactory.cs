using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Loader;

namespace Xcaciv.Command;

/// <summary>
/// Default command factory that supports optional dependency injection resolution.
/// Provides both sync and async methods for command instantiation.
/// Leverages Xcaciv.Loader 2.1.1 instance-based security.
/// </summary>
public class CommandFactory : ICommandFactory
{
    private readonly IServiceProvider? _serviceProvider;
    private AssemblySecurityConfiguration _securityConfiguration;

    public CommandFactory(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
        _securityConfiguration = new AssemblySecurityConfiguration();
    }

    /// <summary>
    /// Configure assembly loading security policies.
    /// Leverages Xcaciv.Loader 2.1.1 instance-based security with per-assembly context isolation.
    /// </summary>
    /// <param name="configuration">Security configuration with policy, path restrictions, and allowlists</param>
    public void SetSecurityConfiguration(AssemblySecurityConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));
        configuration.Validate();
        _securityConfiguration = configuration;
    }

    public ICommandDelegate CreateCommand(ICommandDescription commandDescription, IIoContext ioContext)
    {
        if (commandDescription == null) throw new ArgumentNullException(nameof(commandDescription));
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));

        if (commandDescription.SubCommands.Count > 0 &&
            ioContext.Parameters != null &&
            ioContext.Parameters.Length > 0 &&
            commandDescription.SubCommands.TryGetValue(ioContext.Parameters[0].ToUpper(), out var subCommandDescription) &&
            subCommandDescription != null)
        {
            // Contract: SetParameters must be fast and non-blocking. This call uses ConfigureAwait(false)
            // and synchronous wait due to ICommandFactory being synchronous.
            ioContext.SetParameters(ioContext.Parameters[1..]).ConfigureAwait(false).GetAwaiter().GetResult();
            var subCommand = CreateCommand(subCommandDescription.FullTypeName, commandDescription.PackageDescription.FullPath);
            SetParameterFields(subCommand, ioContext);
            return subCommand;
        }

        var command = CreateCommand(commandDescription.FullTypeName, commandDescription.PackageDescription.FullPath);
        SetParameterFields(command, ioContext);
        return command;
    }

    public ICommandDelegate CreateCommand(string fullTypeName, string packagePath)
    {
        if (string.IsNullOrWhiteSpace(fullTypeName))
        {
            throw new InvalidOperationException("Command type name is empty.");
        }

        var commandType = Type.GetType(fullTypeName);
        if (commandType != null)
        {
            if (_serviceProvider?.GetService(commandType) is ICommandDelegate resolvedDelegate)
            {
                return resolvedDelegate;
            }

            return AssemblyContext.ActivateInstance<ICommandDelegate>(commandType);
        }

        if (string.IsNullOrWhiteSpace(packagePath))
        {
            throw new InvalidOperationException($"Command [{fullTypeName}] is not loaded and no assembly was defined.");
        }

        try
        {
            // Xcaciv.Loader 2.1.1: Instance-based security with strict path restrictions
            // Each plugin command is loaded in an isolated context sandboxed to its own directory
            var basePathRestriction = _securityConfiguration.EnforceBasePathRestriction
                ? Path.GetDirectoryName(packagePath) ?? Directory.GetCurrentDirectory()
                : ".";

            // Xcaciv.Loader 2.1.1: Determine effective security policy
            // If AllowReflectionEmit is disabled, force Strict policy regardless of configuration
            var effectivePolicy = _securityConfiguration.AllowReflectionEmit
                ? _securityConfiguration.SecurityPolicy
                : AssemblySecurityPolicy.Strict;

            // Xcaciv.Loader 2.1.1: Create isolated assembly loading context with security policy
            using var context = new AssemblyContext(
                packagePath,
                basePathRestriction: basePathRestriction,
                securityPolicy: effectivePolicy);
                
            return context.CreateInstance<ICommandDelegate>(fullTypeName);
        }
        catch (SecurityException ex)
        {
            throw new InvalidOperationException(
                $"[Xcaciv.Loader 2.1.1] Security violation loading command [{fullTypeName}] from [{packagePath}]: " +
                $"Policy={(_securityConfiguration.AllowReflectionEmit ? _securityConfiguration.SecurityPolicy : AssemblySecurityPolicy.Strict)}, " +
                $"EnforceBasePathRestriction={_securityConfiguration.EnforceBasePathRestriction}, " +
                $"BasePathRestriction={Path.GetDirectoryName(packagePath)}. " +
                $"Details: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is FileNotFoundException or FileLoadException or BadImageFormatException)
        {
            throw new InvalidOperationException(
                $"[Xcaciv.Loader 2.1.1] Failed to load command [{fullTypeName}] from [{packagePath}]: {ex.GetType().Name}. " +
                $"Verify plugin assembly exists and is valid. {ex.Message}", ex);
        }
    }

    public async Task<ICommandDelegate> CreateCommandAsync(ICommandDescription commandDescription, IIoContext ioContext)
    {
        if (commandDescription == null) throw new ArgumentNullException(nameof(commandDescription));
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));

        if (commandDescription.SubCommands.Count > 0 &&
            ioContext.Parameters != null &&
            ioContext.Parameters.Length > 0 &&
            commandDescription.SubCommands.TryGetValue(ioContext.Parameters[0].ToUpper(), out var subCommandDescription) &&
            subCommandDescription != null)
        {
            // Use async SetParameters to avoid blocking
            await ioContext.SetParameters(ioContext.Parameters[1..]).ConfigureAwait(false);
            var command = CreateCommand(subCommandDescription.FullTypeName, commandDescription.PackageDescription.FullPath);
            SetParameterFields(command, ioContext);
            return command;
        }

        var resultCommand = CreateCommand(commandDescription.FullTypeName, commandDescription.PackageDescription.FullPath);
        SetParameterFields(resultCommand, ioContext);
        return resultCommand;
    }

    /// <summary>
    /// Sets public instance fields on the command from named parameters.
    /// Matches parameter names to field names case-insensitively.
    /// </summary>
    /// <param name="command">Command instance to configure</param>
    /// <param name="ioContext">IO context containing parameters</param>
    private static void SetParameterFields(ICommandDelegate command, IIoContext ioContext)
    {
        if (command == null || ioContext?.Parameters == null || ioContext.Parameters.Length == 0)
            return;

        // Process parameters to get typed parameter values
        var processedParameters = (command as AbstractCommand)?.ProcessParameters(ioContext.Parameters, ioContext.HasPipedInput);
        if (processedParameters == null || processedParameters.Count == 0)
            return;

        // Get public instance fields from the command type
        var commandType = command.GetType();
        var publicFields = commandType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        if (publicFields.Length == 0)
            return;

        // Match and set field values from parameters (case-insensitive)
        foreach (var field in publicFields)
        {
            if (processedParameters.TryGetValue(field.Name, out var parameterValue) && 
                parameterValue != null && 
                parameterValue.IsValid)
            {
                try
                {
                    // Get the typed value from the parameter
                    var value = parameterValue.UntypedValue;
                    
                    // Only set if value is not null and field type is compatible
                    if (value != null && field.FieldType.IsAssignableFrom(value.GetType()))
                    {
                        field.SetValue(command, value);
                    }
                }
                catch (Exception)
                {
                    // Silently ignore field setting errors to avoid breaking command execution
                    // This follows the pattern of optional parameter injection
                }
            }
        }
    }
}
