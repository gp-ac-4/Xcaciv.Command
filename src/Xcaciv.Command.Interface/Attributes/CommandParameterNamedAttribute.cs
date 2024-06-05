using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterNamedAttribute : Attribute
    {
        private string _parameterName = "TODO";

        public CommandParameterNamedAttribute(string name, string description) 
        { 
            this.Name = name;
            this.ValueDescription = description;
        }


        public string ShortAlias { get; } = "";


        public string Name
        {
            get { return _parameterName; }
            set { _parameterName = CommandDescription.GetValidCommandName(value, false); }
        }

        public string ValueDescription { get; set; } = "";
        public bool IsRequired { get; set; } = false;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// </summary>
        public bool UsePipe { get; set; } = false;
        public override string ToString()
        {
            var prameterName = $"{ShortAlias} {_parameterName}";
            return $"{prameterName,18} {ValueDescription}".Trim();
        }
    }
}
