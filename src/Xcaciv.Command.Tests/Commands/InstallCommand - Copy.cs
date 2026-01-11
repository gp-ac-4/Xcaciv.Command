using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Packages
{
    [CommandRegister("search", "Search for command packages using natural language terms")]
    [CommandRoot("package", "Manage command packages")]
    [CommandParameterOrdered("terms", "Search terms")]
    [CommandParameterNamed("take", "Number of results to return (default 20, max 100)", DataType = typeof(int))]
    [CommandFlag("prerelease", "Include prerelease packages", DataType = typeof(bool))]
    [CommandParameterNamed("source", "Package source URL (HTTPS)")]
    [CommandParameterNamed("verbosity", "quiet|normal|detailed", AllowedValues = ["quiet", "normal", "detailed"])]
    public class TestSubCommand : AbstractCommand
    {
        public override IResult<string> HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var paramNames = string.Join(',', parameters.Keys);
            return CommandResult<string>.Success("Not installing " + paramNames);
        }

        public override IResult<string> HandlePipedChunk(IResult<string> pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            var paramNames = string.Join(',', parameters.Keys);
            return CommandResult<string>.Success($"Not installing {pipedChunk.Output} " + paramNames);
        }
    }
}
