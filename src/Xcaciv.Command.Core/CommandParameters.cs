using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Core;

public static class CommandParameters
{

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

    public static void ProcessOrderedParameters(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandParameterOrderedAttribute[] commandParametersOrdered)
    {
        foreach (var parameter in commandParametersOrdered)
        {
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

    public static void ProcessSuffixParameters(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandParameterSuffixAttribute[] commandParametersSuffix)
    {
        foreach (var parameter in commandParametersSuffix)
        {
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
}