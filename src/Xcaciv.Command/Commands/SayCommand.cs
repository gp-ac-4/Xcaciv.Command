using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Commands
{
    public class SayCommand : AbstractCommand
    {
        public override string BaseCommand { get; } = "SAY";


        public override string FriendlyName { get; } = "say something";


        public override string HelpString { get; } = "SAY <thing to print>";

        public override string HandleExecution(string[] parameters, IStatusContext status)
        {
            return String.Join(" ", parameters);
        }

        public override string HandlePipedChunk(string pipedChunk, string[] parameters, IStatusContext status)
        {
            return pipedChunk;
        }

    }
}
