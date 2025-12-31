using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;
using Xcaciv.Command.Interface.Parameters;

namespace zTestCommandPackage
{
    [CommandRoot("do", "does stuff")]
    [CommandRegister("SAY", "a funny test sub command like echo", Prototype ="do say <some text>")]
    [CommandParameterSuffix("text", "Text to say")]
    public class DoSayCommand : AbstractCommand
    {
        public override string HandleExecution(Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            if (parameters.TryGetValue("text", out var textParam) && textParam.IsValid)
            {
                return textParam.RawValue;
            }
            return String.Join(' ', parameters.Values.Select(p => p.RawValue));
        }

        public override string HandlePipedChunk(string pipedChunk, Dictionary<string, IParameterValue> parameters, IEnvironmentContext env)
        {
            return pipedChunk + String.Join(' ', parameters.Values.Select(p => p.RawValue));
        }
    }
}
