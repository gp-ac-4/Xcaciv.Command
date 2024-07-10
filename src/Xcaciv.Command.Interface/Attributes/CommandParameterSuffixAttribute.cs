using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterSuffixAttribute : AbstractCommandParameter
    {

        public CommandParameterSuffixAttribute(string name, string description) 
        { 
            this.Name = name;
            this.ValueDescription = description;
        }
        /// <summary>
        /// used when no value is provided
        /// this satisfies the IsRequired flag
        /// </summary>
        public string DefaultValue { get; set; } = "";
        /// <summary>
        /// specify if the value is required
        /// </summary>
        public bool IsRequired { get; set; } = true;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// </summary>
        public bool UsePipe { get; set; } = false;
    }
}
