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
            this.ParameterName = name;
            this.ValueDescription = description;
        }


        public string ShortAlias { get; } = "";


        public string ParameterName
        {
            get { return _parameterName; }
            set { _parameterName = CommandDescription.InvalidParameterChars.Replace(value, "").Trim().ToLower(); }
        }

        public string ValueDescription { get; set; } = "";

        public override string ToString()
        {
            var prameterName = $"{ShortAlias} {_parameterName}";
            return $"{prameterName,15} {ValueDescription}".Trim();
        }
    }
}
