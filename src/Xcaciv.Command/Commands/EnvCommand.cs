using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command.Commands
{
    [BaseCommand("ENV", "output all environment variables", Prototype = "ENV")]
    internal class EnvCommand : AbstractCommand
    {
        public override string HandleExecution(string[] parameters, IEnvironment env)
        {
            var values = String.Empty;
            foreach (var valuePair in env.GetEnvinronment())
            {
                values += @$"{valuePair.Key} = {valuePair.Value}\n";
            }
            return values;
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IEnvironment env)
        {
            var values = String.Empty;
            foreach (var valuePair in env.GetEnvinronment())
            {
                values += @$"{valuePair.Key} = {valuePair.Value}\n";
            }
            return values;
        }
    }
}
