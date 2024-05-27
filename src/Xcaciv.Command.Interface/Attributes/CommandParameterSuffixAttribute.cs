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
            this.Name = name;
            this.ValueDescription = description;
        }


        public string Name
        {
            get { return _helpName; }
            set { _helpName = CommandDescription.GetValidCommandName(value, false); }
        }

        public string ValueDescription { get; set; } = "";
        public bool IsRquired { get; set; } = true;

        public override string ToString()
        {
            var placeholder = $"<{_helpName}>";
            return $"{placeholder,18} {ValueDescription}".Trim();
        }
    }
}
