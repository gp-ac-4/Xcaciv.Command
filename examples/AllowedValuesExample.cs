using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;
using System.Collections.Generic;

namespace Xcaciv.Command.Examples
{
    /// <summary>
    /// Example command demonstrating AllowedValues validation and auto-default features
    /// </summary>
    [CommandRegister("BUILD", "Build project with specified configuration")]
    [CommandParameterNamed(
        "configuration", 
        "Build configuration",
        AllowedValues = new[] { "Debug", "Release", "Test" }
        // DefaultValue automatically set to "Debug" (first allowed value)
    )]
    [CommandParameterNamed(
        "verbosity", 
        "Output verbosity level",
        AllowedValues = new[] { "quiet", "minimal", "normal", "detailed" },
        DefaultValue = "normal"  // Explicitly set, validated against AllowedValues
    )]
    [CommandParameterOrdered(
        "target",
        "Target framework",
        AllowedValues = new[] { "net10.0", "net8.0", "net6.0" },
        IsRequired = false
        // DefaultValue automatically set to "net10.0"
    )]
    public class BuildCommand : AbstractCommand
    {
        // Auto-populated by field injection
        public string? Configuration;
        public string? Verbosity;
        public string? Target;

        public override IResult<string> HandleExecution(
            Dictionary<string, IParameterValue> parameters, 
            IEnvironmentContext env)
        {
            // Values are guaranteed to be from AllowedValues or the validated DefaultValue
            var config = Configuration ?? "Debug";  // Will always be one of: Debug, Release, Test
            var verb = Verbosity ?? "normal";       // Will always be one of: quiet, minimal, normal, detailed
            var target = Target ?? "net10.0";       // Will always be one of: net10.0, net8.0, net6.0

            var output = $"Building with configuration={config}, verbosity={verb}, target={target}";
            
            return CommandResult<string>.Success(output, OutputFormat);
        }
    }
}

/*
 * Example Usage:
 * 
 * BUILD                                    ? Uses defaults: Debug, normal, net10.0
 * BUILD -configuration Release             ? Uses: Release, normal, net10.0
 * BUILD -verbosity quiet -configuration Test ? Uses: Test, quiet, net10.0
 * BUILD net8.0 -configuration Release      ? Uses: Release, normal, net8.0
 * 
 * Invalid Usage (throws validation error):
 * BUILD -configuration Invalid             ? Error: "Invalid" not in allowed values
 * BUILD -verbosity debug                   ? Error: "debug" not in allowed values
 */
