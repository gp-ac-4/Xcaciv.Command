using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace zTestCommandPackage
{
    public class SayCommand : ICommand
    {
        public string BaseCommand => "SAY";

        public string FriendlyName => "Say";

        public ValueTask DisposeAsync()
        {
            // nothing to dispose
            return ValueTask.CompletedTask;
        }

        public Task Help(ITextIoContext messageContext)
        {
            return messageContext.WriteLine("Say <something>");
        }

        public Task<string> Main(string[] parameters, ITextIoContext messageContext)
        {
            messageContext.SetStatusMessage("Say command executed.");
            return Task.FromResult(String.Join(' ', parameters));
        }
    }
}
