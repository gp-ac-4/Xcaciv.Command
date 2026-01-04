using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;
using Xcaciv.Command.Core.Parameters;

namespace Xcaciv.Command.Core;

/// <summary>
/// Provides attribute-driven command parameter parsing and normalization for ordered, flag, named, and suffix parameters.
/// This utility is used by command implementations to transform raw argument arrays into typed parameter dictionaries.
/// </summary>
public static class CommandParameters
{
    private static readonly IParameterConverter DefaultConverter = new DefaultParameterConverter();

    /// <summary>
    /// Extracts boolean flag values from the raw parameter list and records their presence in the lookup map.
    /// Modifies <paramref name="parameterList"/> by removing matched flag tokens.
    /// </summary>
    /// <param name="parameterList">Mutable list of raw parameters to scan and mutate.</param>
    /// <param name="parameterLookup">Destination map that records flag presence as string values.</param>
    /// <param name="commandFlagAttributes">Flag definitions from command attributes.</param>
    public static void ProcessFlags(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandFlagAttribute[] commandFlagAttributes)
    {
        foreach (var parameter in commandFlagAttributes)
        {
            var index = 0;
            Regex fullName = new Regex("-{1,2}" + parameter.Name);
            Regex abbrName = string.IsNullOrEmpty(parameter.ShortAlias) ? new Regex("^$") :
                new Regex("-{1,2}" + parameter.ShortAlias);

            var found = false;

            foreach (var value in parameterList)
            {
                if (value.StartsWith("-") && (fullName.IsMatch(value) || abbrName.IsMatch(value)))
                {
                    found = true;
                    parameterList.RemoveAt(index);
                    break;
                }
                index++;
            }

            parameterLookup.Add(parameter.Name, found.ToString());
        }
    }

    /// <summary>
    /// Parses named key/value parameters from the raw parameter list, applying defaults and required checks.
    /// Removes consumed tokens from <paramref name="parameterList"/> and populates the lookup map.
    /// </summary>
    public static void ProcessNamedParameters(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandParameterNamedAttribute[] commandParametersNamed)
    {
        foreach (var parameter in commandParametersNamed)
        {
            var index = 0;
            Regex fullName = new Regex("-{1,2}" + parameter.Name);
            Regex abbrName = string.IsNullOrEmpty(parameter.ShortAlias) ? new Regex("^$") :
                new Regex("-{1,2}" + parameter.ShortAlias);

            var found = false;
            var foundValue = string.Empty;

            foreach (var value in parameterList)
            {
                if (value.StartsWith("-") && (fullName.IsMatch(value) || abbrName.IsMatch(value)))
                {
                    var valueIndex = index + 1;
                    foundValue = parameterList[valueIndex];
                    parameterList.RemoveAt(valueIndex);
                    parameterList.RemoveAt(index);
                    found = true;
                    break;
                }
                index++;
            }

            if (!found)
            {
                if (parameter.DefaultValue != string.Empty)
                {
                    foundValue = parameter.DefaultValue;
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
            }

            if (parameter.AllowedValues.Count() > 0 &&
                !parameter.AllowedValues.Contains(foundValue, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid value for parameter {parameter.Name}, this parameter has an allow list.");
            }            

            parameterLookup.Add(parameter.Name, foundValue);
        }
    }

    /// <summary>
    /// Parses ordered (positional) parameters from the front of the parameter list, applying defaults and allow-list validation.
    /// Mutates <paramref name="parameterList"/> as ordered values are consumed.
    /// </summary>
    public static void ProcessOrderedParameters(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandParameterOrderedAttribute[] commandParametersOrdered)
    {
        foreach (var parameter in commandParametersOrdered)
        {
            // Check if parameter list is empty before accessing index 0
            if (parameterList.Count == 0)
            {
                if (!parameter.IsRequired && !string.IsNullOrEmpty(parameter.DefaultValue))
                {
                    parameterLookup.Add(parameter.Name, parameter.DefaultValue);
                    continue;
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
                continue;
            }

            var foundValue = parameterList[0];
            if (String.IsNullOrEmpty(foundValue) && !String.IsNullOrEmpty(parameter.DefaultValue))
            {
                foundValue = parameter.DefaultValue;
            }

            // If the current parameter is a named parameter, then need to check if we have any unsatisfied
            // named parameters. This does preclude any negative numbers as valid unnamed parameters.
            if (foundValue.StartsWith('-')) 
            {
                if (String.IsNullOrEmpty(parameter.DefaultValue) && parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
            }
            else
            {
                if (parameter.AllowedValues.Count() > 0 &&
                    !parameter.AllowedValues.Contains(foundValue, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid value for parameter {parameter.Name}, this parameter has an allow list.");
                }

                parameterLookup.Add(parameter.Name, foundValue);
                parameterList.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Captures trailing arguments (suffix parameters), handling defaults and required enforcement. Consumes remaining parameters.
    /// </summary>
    public static void ProcessSuffixParameters(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandParameterSuffixAttribute[] commandParametersSuffix)
    {
        foreach (var parameter in commandParametersSuffix)
        {
            // Check if parameter list is empty before accessing index 0
            if (parameterList.Count == 0)
            {
                if (!parameter.IsRequired && !string.IsNullOrEmpty(parameter.DefaultValue))
                {
                    parameterLookup.Add(parameter.Name, parameter.DefaultValue);
                    continue;
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
                continue;
            }

            // if the current parameter is a named parameter, then need to check if we have any
            // unsatisfied named parameters
            if (parameterList[0].StartsWith("-"))
            {
                if (parameter.DefaultValue != string.Empty)
                {
                    parameterLookup.Add(parameter.Name, parameter.DefaultValue);
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }

                continue;
            }

            parameterLookup.Add(parameter.Name, parameterList[0]);
            parameterList.RemoveAt(0);
        }
    }

    /// <summary>
    /// Builds a command description from attributes for registration, optionally populating an existing description when handling sub-commands.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the command type lacks a required <see cref="CommandRegisterAttribute"/>.</exception>
    public static ICommandDescription CreatePackageDescription(Type commandType, PackageDescription packagDesc, ICommandDescription? description = null)
    {
        // required to have BaseCommandAttribute, 
        if (Attribute.GetCustomAttribute(commandType, typeof(CommandRegisterAttribute)) is CommandRegisterAttribute attributes)
        {
            if (Attribute.GetCustomAttribute(commandType, typeof(CommandRootAttribute)) is CommandRootAttribute rootAttribute)
            {
                // this command is a sub command, add it to the parent
                if (description == null)
                {
                    return new CommandDescription()
                    {
                        BaseCommand = rootAttribute.Command,
                        PackageDescription = packagDesc,
                        SubCommands = new Dictionary<string, ICommandDescription>()
                                    {
                                        { attributes.Command,
                                        new CommandDescription()
                                        {
                                            BaseCommand = attributes.Command,
                                            FullTypeName = commandType.FullName ?? String.Empty,
                                            PackageDescription = packagDesc
                                        } }
                                    }
                    };
                }
                else
                {
                    description.SubCommands[attributes.Command] = new CommandDescription()
                    {
                        BaseCommand = attributes.Command,
                        FullTypeName = commandType.FullName ?? String.Empty,
                        PackageDescription = packagDesc
                    };
                    return description;
                }
            }
            
            // this is a root command
            return new CommandDescription()
            {
                BaseCommand = attributes.Command,
                FullTypeName = commandType.FullName ?? String.Empty,
                PackageDescription = packagDesc
            };         

        }
        else
        {
            throw new InvalidOperationException($"{commandType.FullName} implements ICommandDelegate but does not have BaseCommandAttribute. Unable to automatically register.");
        }

    }

    /// <summary>
    /// Parses raw parameters into typed parameter values using command parameter attributes for ordered, flag, named, and suffix arguments.
    /// </summary>
    /// <param name="parameters">Raw parameter array from the caller.</param>
    /// <param name="orderedAttrs">Ordered parameter definitions.</param>
    /// <param name="flagAttrs">Flag parameter definitions.</param>
    /// <param name="namedAttrs">Named parameter definitions.</param>
    /// <param name="suffixAttrs">Suffix parameter definitions.</param>
    /// <returns>Dictionary keyed by parameter name with typed values and validation metadata.</returns>
    public static Dictionary<string, IParameterValue> ProcessTypedParameters(
        string[] parameters,
        CommandParameterOrderedAttribute[] orderedAttrs,
        CommandFlagAttribute[] flagAttrs,
        CommandParameterNamedAttribute[] namedAttrs,
        CommandParameterSuffixAttribute[] suffixAttrs)
    {
        var parameterLookup = new Dictionary<string, IParameterValue>(StringComparer.OrdinalIgnoreCase);
        var parameterList = parameters.ToList();

        ProcessTypedOrderedParameters(parameterList, parameterLookup, orderedAttrs);
        ProcessTypedFlags(parameterList, parameterLookup, flagAttrs);
        ProcessTypedNamedParameters(parameterList, parameterLookup, namedAttrs);
        ProcessTypedSuffixParameters(parameterList, parameterLookup, suffixAttrs);

        return parameterLookup;
    }

    /// <summary>
    /// Helper method to create a ParameterValue using the converter.
    /// </summary>
    private static IParameterValue CreateParameterValue(string name, string rawValue, Type targetType)
    {
        var convertedValue = DefaultConverter.ValidateAndConvert(name, rawValue, targetType, out var validationError, out var isValid);
        return ParameterValue.Create(name, rawValue, convertedValue, targetType, isValid, validationError);
    }

    /// <summary>
    /// Parses boolean flags and records typed values in the lookup dictionary. Consumes matched tokens from the working list.
    /// </summary>
    private static void ProcessTypedFlags(
        List<string> parameterList,
        Dictionary<string, IParameterValue> parameterLookup,
        CommandFlagAttribute[] commandFlagAttributes)
    {
        foreach (var parameter in commandFlagAttributes ?? Array.Empty<CommandFlagAttribute>())
        {
            var index = 0;
            Regex fullName = new Regex("-{1,2}" + parameter.Name);
            Regex abbrName = string.IsNullOrEmpty(parameter.ShortAlias) ? new Regex("^$") :
                new Regex("-{1,2}" + parameter.ShortAlias);

            var found = false;

            foreach (var value in parameterList)
            {
                if (value.StartsWith("-") && (fullName.IsMatch(value) || abbrName.IsMatch(value)))
                {
                    found = true;
                    parameterList.RemoveAt(index);
                    break;
                }
                index++;
            }

            var boolValue = found ? "true" : "false";
            parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, boolValue, typeof(bool));
        }
    }

    /// <summary>
    /// Parses named parameters into typed values, validating required/default behavior and allow lists.
    /// </summary>
    private static void ProcessTypedNamedParameters(
        List<string> parameterList,
        Dictionary<string, IParameterValue> parameterLookup,
        CommandParameterNamedAttribute[] commandParametersNamed)
    {
        foreach (var parameter in commandParametersNamed ?? Array.Empty<CommandParameterNamedAttribute>())
        {
            var index = 0;
            Regex fullName = new Regex("-{1,2}" + parameter.Name);
            Regex abbrName = string.IsNullOrEmpty(parameter.ShortAlias) ? new Regex("^$") :
                new Regex("-{1,2}" + parameter.ShortAlias);

            var found = false;
            var foundValue = string.Empty;

            foreach (var value in parameterList)
            {
                if (value.StartsWith("-") && (fullName.IsMatch(value) || abbrName.IsMatch(value)))
                {
                    var valueIndex = index + 1;
                    foundValue = parameterList[valueIndex];
                    parameterList.RemoveAt(valueIndex);
                    parameterList.RemoveAt(index);
                    found = true;
                    break;
                }
                index++;
            }

            if (!found)
            {
                if (parameter.DefaultValue != string.Empty)
                {
                    foundValue = parameter.DefaultValue;
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
            }

            if (parameter.AllowedValues.Count() > 0 &&
                !parameter.AllowedValues.Contains(foundValue, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid value for parameter {parameter.Name}, this parameter has an allow list.");
            }

            parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, foundValue, parameter.DataType);
        }
    }

    /// <summary>
    /// Parses ordered parameters into typed values, validating required/default behavior and allow lists.
    /// </summary>
    private static void ProcessTypedOrderedParameters(
        List<string> parameterList,
        Dictionary<string, IParameterValue> parameterLookup,
        CommandParameterOrderedAttribute[] commandParametersOrdered)
    {
        foreach (var parameter in commandParametersOrdered ?? Array.Empty<CommandParameterOrderedAttribute>())
        {
            // Check if parameter list is empty before accessing index 0
            if (parameterList.Count == 0)
            {
                if (!parameter.IsRequired && !string.IsNullOrEmpty(parameter.DefaultValue))
                {
                    parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, parameter.DefaultValue, parameter.DataType);
                    continue;
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
                continue;
            }

            var foundValue = parameterList[0];
            if (String.IsNullOrEmpty(foundValue) && !String.IsNullOrEmpty(parameter.DefaultValue))
            {
                foundValue = parameter.DefaultValue;
            }

            // If the current parameter is a named parameter, then need to check if we have any unsatisfied
            // named parameters. This does preclude any negative numbers as valid unnamed parameters.
            if (foundValue.StartsWith('-'))
            {
                if (String.IsNullOrEmpty(parameter.DefaultValue) && parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
            }
            else
            {
                if (parameter.AllowedValues.Count() > 0 &&
                    !parameter.AllowedValues.Contains(foundValue, StringComparer.OrdinalIgnoreCase))
                {
                    throw new ArgumentException($"Invalid value for parameter {parameter.Name}, this parameter has an allow list.");
                }

                parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, foundValue, parameter.DataType);
                parameterList.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Parses suffix parameters into typed values, collapsing remaining arguments into a single value when appropriate.
    /// </summary>
    private static void ProcessTypedSuffixParameters(
        List<string> parameterList,
        Dictionary<string, IParameterValue> parameterLookup,
        CommandParameterSuffixAttribute[] commandParametersSuffix)
    {
        foreach (var parameter in commandParametersSuffix ?? Array.Empty<CommandParameterSuffixAttribute>())
        {
            // Check if parameter list is empty before accessing index 0
            if (parameterList.Count == 0)
            {
                if (!parameter.IsRequired && !string.IsNullOrEmpty(parameter.DefaultValue))
                {
                    parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, parameter.DefaultValue, parameter.DataType);
                    continue;
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }
                continue;
            }

            // if the current parameter is a named parameter, then need to check if we have any
            // unsatisfied named parameters
            if (parameterList[0].StartsWith("-"))
            {
                if (parameter.DefaultValue != string.Empty)
                {
                    parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, parameter.DefaultValue, parameter.DataType);
                }
                else if (parameter.IsRequired)
                {
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");
                }

                continue;
            }

            // Join all remaining parameters into a single string
            var combinedValue = string.Join(" ", parameterList);
            parameterLookup[parameter.Name] = CreateParameterValue(parameter.Name, combinedValue, parameter.DataType);
            parameterList.Clear(); // Remove all items since they're all combined into one parameter
        }
    }
}