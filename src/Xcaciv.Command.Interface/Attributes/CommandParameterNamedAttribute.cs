using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterNamedAttribute : AbstractCommandParameter
    {
        public CommandParameterNamedAttribute(string name, string description) 
        { 
            this.Name = name;
            this.ValueDescription = description;
        }
        /// <summary>
        /// used when no value is provided
        /// this satisfies the IsRequired flag
        /// </summary>
        public string DefaultValue { get; set; } = "";

        public string ShortAlias { get; set; } = "";
        /// <summary>
        /// throws an error if this value is not provided
        /// </summary>
        public bool IsRequired { get; set; } = false;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// when multiple are specified
        /// </summary>
        public bool UsePipe { get; set; } = false;

        /// <summary>
        /// input values that are allowed, anything else will throw an error
        /// case is ignored
        /// </summary>
        public string[] AllowedValues { get; set; } = [];


        public override string GetIndicator()
        {
            return $"-{_helpName}";
        }

        public override string GetValueDescription()
        {
            string description = ValueDescription;
            if (AllowedValues.Count() > 0)
            {
                description += $" (Allowed values: {string.Join(", ", AllowedValues)})";
            }
            return description;
        }
    }
}
