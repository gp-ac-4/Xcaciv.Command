using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    /// <summary>
    /// this attribute indicates a toggle flag for a command
    /// if the flag is present the value is true
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandFlagAttribute : Attribute
    {
        private string _parameterName = "TODO";
        /// <summary>
        /// this attribute indicates a toggle flag for a command
        /// if the flag is present the value is true
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public CommandFlagAttribute(string name, string description) 
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

        public override string ToString()
        {
            var prameterName = $"{ShortAlias} {_parameterName}";
            return $"{prameterName,18} {ValueDescription}".Trim();
        }
    }
}
