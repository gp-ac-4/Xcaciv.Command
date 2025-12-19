using System;
using System.IO;
using System.Security;
using Xcaciv.Command.Interface;
using Xcaciv.Loader;

namespace Xcaciv.Command;

/// <summary>
/// Default command factory that supports optional dependency injection resolution.
/// </summary>
public class CommandFactory : ICommandFactory
{
    private readonly IServiceProvider? _serviceProvider;

    public CommandFactory(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
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
            return CreateCommand(subCommandDescription.FullTypeName, commandDescription.PackageDescription.FullPath);
        }

        return CreateCommand(commandDescription.FullTypeName, commandDescription.PackageDescription.FullPath);
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
            using var context = new AssemblyContext(
                packagePath,
                basePathRestriction: Path.GetDirectoryName(packagePath) ?? Directory.GetCurrentDirectory(),
                securityPolicy: AssemblySecurityPolicy.Default);
            return context.CreateInstance<ICommandDelegate>(fullTypeName);
        }
        catch (SecurityException ex)
        {
            throw new InvalidOperationException($"Security violation loading command [{fullTypeName}] from [{packagePath}]: {ex.Message}", ex);
        }
    }
}
