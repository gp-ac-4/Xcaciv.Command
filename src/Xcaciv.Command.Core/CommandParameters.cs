using System.Text.RegularExpressions;
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

            foreach (var value in parameterList)
            {
                if (value.StartsWith("-") && (fullName.IsMatch(value) || abbrName.IsMatch(value)))
                {
                    var valueIndex = index + 1;
                    parameterLookup.Add(parameter.Name, parameterList[valueIndex]);
                    parameterList.RemoveAt(valueIndex);
                    parameterList.RemoveAt(index);
                    break;
                }
                index++;
            }
        }
    }

    public static void ProcessOrderedParameters(List<string> parameterList, Dictionary<string, string> parameterLookup, CommandParameterOrderedAttribute[] commandParametersOrdered)
    {
        foreach (var parameter in commandParametersOrdered)
        {
            // if the current parameter is a named parameter, then need to check if we have any
            // unsatisfied named parameters
            if (parameterList[0].StartsWith("-"))
            {
                if (parameter.IsRquired)
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");

                continue;
            }

            parameterLookup.Add(parameter.Name, parameterList[0]);
            parameterList.RemoveAt(0);
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
                if (parameter.IsRquired)
                    throw new ArgumentException($"Missing required parameter {parameter.Name}");

                continue;
            }

            parameterLookup.Add(parameter.Name, parameterList[0]);
            parameterList.RemoveAt(0);
        }
    }
}