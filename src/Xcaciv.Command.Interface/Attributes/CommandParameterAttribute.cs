using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterAttribute : Attribute
    {
        private string _valueName = "TODO";

        public CommandParameterAttribute(string name, string description) 
        { 
            this.ValueName = name;
        }

        public CommandParameterAttribute(string flagname, string name, string description)
        {
            this.FlagName = flagname;
            this.ValueName = name;
        }

        public CommandParameterAttribute(string abbrFlag, string flagname, string name, string description)
        {
            this.AbbrFlagName = abbrFlag;
            this.FlagName = flagname;
            this.ValueName = name;
        }

        public string AbbrFlagName { get; } = "";
        public string FlagName { get; set; } = "";

        public string ValueName
        {
            get { return _valueName; }
            set { _valueName = String.Format(@"<{0}>", value.Trim('>').Trim(']')); }
        }

        public string ValueDescription { get; set; } = "";

        public override string ToString()
        {
            return $"{AbbrFlagName} {FlagName} \t {_valueName} \t {ValueDescription}".Trim();
        }
    }
}
