using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterSuffixAttribute : Attribute
    {
        private string _helpName = "TODO";

        public CommandParameterSuffixAttribute(string name, string description) 
        { 
            this.HelpName = name;
            this.ValueDescription = description;
        }


        public string HelpName
        {
            get { return _helpName; }
            set { _helpName = String.Format(@"<{0}>", value.Trim('>').Trim(']').ToLower()); }
        }

        public string ValueDescription { get; set; } = "";

        public override string ToString()
        {
            return $"{_helpName,12} {ValueDescription}".Trim();
        }
    }
}
