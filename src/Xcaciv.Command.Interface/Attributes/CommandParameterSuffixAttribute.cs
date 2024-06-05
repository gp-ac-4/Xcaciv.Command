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
        public bool IsRequired { get; set; } = true;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// </summary>
        public bool UsePipe { get; set; } = false;
        public override string ToString()
        {
            var placeholder = $"<{_helpName}>";
            return $"{placeholder,18} {ValueDescription}".Trim();
        }
    }
}
