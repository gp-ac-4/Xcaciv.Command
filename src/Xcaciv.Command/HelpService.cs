using System;
using System.Linq;
using System.Text;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command;

/// <summary>
/// Default implementation of IHelpService that formats help output from command metadata via reflection.
/// Extracts metadata from command attributes and produces standardized help output.
/// </summary>
public class HelpService : IHelpService
{
    public string BuildHelp(ICommandDelegate command, string[] parameters, IEnvironmentContext environment)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        // For backward compatibility, if command has overridden Help(), use it
        // This allows gradual migration to attribute-based help
        var helpMethod = command.GetType().GetMethod("Help");
        if (helpMethod != null && helpMethod.DeclaringType != typeof(object))
        {
            var helpResult = command.Help(parameters, environment);
            if (!string.IsNullOrEmpty(helpResult))
            {
                return helpResult;
            }
        }

        // Otherwise, build help from attributes
        var commandType = command.GetType();
        var baseCommand = Attribute.GetCustomAttribute(commandType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
        
        if (baseCommand == null)
        {
            throw new InvalidOperationException("CommandRegisterAttribute is required for all commands");
        }

        var commandParametersOrdered = GetOrderedParameters(commandType, false);
        var commandParametersFlag = GetFlagParameters(commandType);
        var commandParametersNamed = GetNamedParameters(commandType, false);
        var commandParametersSuffix = GetSuffixParameters(commandType, false);
        var helpRemarks = Attribute.GetCustomAttributes(commandType, typeof(CommandHelpRemarksAttribute)) as CommandHelpRemarksAttribute[];

        var builder = new StringBuilder();
        
        // Command name with parent if applicable
        if (Attribute.GetCustomAttribute(commandType, typeof(CommandRootAttribute)) is CommandRootAttribute rootCommand)
        {
            builder.Append($"{rootCommand.Command} ");
        }
        
        builder.AppendLine($"{baseCommand.Command}:");
        builder.AppendLine($"  {baseCommand.Description}");
        builder.AppendLine();
        builder.AppendLine("Usage:");
        
        if (commandParametersOrdered.Length + commandParametersNamed.Length + commandParametersSuffix.Length + commandParametersFlag.Length > 0)
        {
            var parameterBuilder = new StringBuilder();
            var prototypeBuilder = new StringBuilder();

            // Ordered parameters
            foreach (var parameter in commandParametersOrdered)
            {
                parameterBuilder.AppendLine($"  {parameter}");
                prototypeBuilder.Append($"{parameter.GetIndicator()} ");
            }

            // Flag parameters
            foreach (var parameter in commandParametersFlag)
            {
                parameterBuilder.AppendLine($"  {parameter}");
                prototypeBuilder.Append($"{parameter.GetIndicator()} ");
            }

            // Named parameters
            foreach (var parameter in commandParametersNamed)
            {
                parameterBuilder.AppendLine($"  {parameter}");
                string valueIndicator = (parameter.AllowedValues.Length > 0) ?
                    $"[{string.Join("|", parameter.AllowedValues)}]" :
                    parameter.Name;

                prototypeBuilder.Append($"{parameter.GetIndicator()} <{valueIndicator}> ");
            }

            // Suffix parameters
            foreach (var parameter in commandParametersSuffix)
            {
                parameterBuilder.AppendLine($"  {parameter}");
                prototypeBuilder.Append($"{parameter.GetIndicator()} ");
            }

            // Use prototype from attribute or generated one
            if (baseCommand.Prototype.Equals("todo", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendLine(prototypeBuilder.ToString());
            }
            else
            {
                builder.AppendLine($"  {baseCommand.Prototype}");
            }

            builder.AppendLine();
            builder.AppendLine("Options:");
            builder.AppendLine(parameterBuilder.ToString());
        }

        // Remarks section
        if (helpRemarks != null && helpRemarks.Length > 0)
        {
            builder.AppendLine("Remarks:");
            foreach (var remark in helpRemarks)
            {
                builder.AppendLine();
                builder.AppendLine(remark.Remarks);
            }
        }

        builder.AppendLine();
        return builder.ToString();
    }

    public string BuildOneLineHelp(ICommandDescription commandDescription)
    {
        if (commandDescription == null) throw new ArgumentNullException(nameof(commandDescription));

        if (commandDescription.SubCommands.Count > 0)
        {
            return $"-\t{commandDescription.BaseCommand,-12} [Has sub-commands]";
        }

        // Try to get the command type - first try Type.GetType (works for built-in types)
        var commandType = Type.GetType(commandDescription.FullTypeName);
        
        // If Type.GetType fails (common for plugin types), try loading from assembly
        if (commandType == null && !string.IsNullOrEmpty(commandDescription.PackageDescription?.FullPath))
        {
            try
            {
                var assembly = System.Reflection.Assembly.LoadFrom(commandDescription.PackageDescription.FullPath);
                commandType = assembly.GetType(commandDescription.FullTypeName);
            }
            catch
            {
                // If assembly load fails, fall through to default
            }
        }

        if (commandType != null)
        {
            // Check if command has CommandRootAttribute (indicates it's a sub-command)
            if (Attribute.GetCustomAttribute(commandType, typeof(CommandRootAttribute)) is CommandRootAttribute)
            {
                var baseCommand = Attribute.GetCustomAttribute(commandType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
                return $"-\t{commandDescription.BaseCommand,-12} {baseCommand?.Description ?? ""}";
            }

            // For regular commands, use CommandRegisterAttribute
            var registerAttr = Attribute.GetCustomAttribute(commandType, typeof(CommandRegisterAttribute)) as CommandRegisterAttribute;
            if (registerAttr != null)
            {
                return $"{registerAttr.Command,-12} {registerAttr.Description}";
            }
        }

        return $"{commandDescription.BaseCommand,-12} [No description available]";
    }

    public bool IsHelpRequest(string[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
        {
            return false;
        }

        return parameters.Any(p => p.Equals("--HELP", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals("-?", StringComparison.OrdinalIgnoreCase) ||
                                   p.Equals("/?", StringComparison.OrdinalIgnoreCase));
    }

    private static CommandParameterOrderedAttribute[] GetOrderedParameters(Type commandType, bool hasPipedInput)
    {
        var ordered = Attribute.GetCustomAttributes(commandType, typeof(CommandParameterOrderedAttribute)) as CommandParameterOrderedAttribute[] ?? Array.Empty<CommandParameterOrderedAttribute>();
        if (hasPipedInput)
        {
            ordered = ordered.Where(x => !x.UsePipe).ToArray();
        }
        return ordered;
    }

    private static CommandParameterNamedAttribute[] GetNamedParameters(Type commandType, bool hasPipedInput)
    {
        var named = Attribute.GetCustomAttributes(commandType, typeof(CommandParameterNamedAttribute)) as CommandParameterNamedAttribute[] ?? Array.Empty<CommandParameterNamedAttribute>();
        if (hasPipedInput)
        {
            named = named.Where(x => !x.UsePipe).ToArray();
        }
        return named;
    }

    private static CommandFlagAttribute[] GetFlagParameters(Type commandType)
    {
        return Attribute.GetCustomAttributes(commandType, typeof(CommandFlagAttribute)) as CommandFlagAttribute[] ?? Array.Empty<CommandFlagAttribute>();
    }

    private static CommandParameterSuffixAttribute[] GetSuffixParameters(Type commandType, bool hasPipedInput)
    {
        var suffix = Attribute.GetCustomAttributes(commandType, typeof(CommandParameterSuffixAttribute)) as CommandParameterSuffixAttribute[] ?? Array.Empty<CommandParameterSuffixAttribute>();
        if (hasPipedInput)
        {
            suffix = suffix.Where(x => !x.UsePipe).ToArray();
        }
        return suffix;
    }
}
