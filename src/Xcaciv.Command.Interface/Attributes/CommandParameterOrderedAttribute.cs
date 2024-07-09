using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    /// <summary>
    /// An unnamed parameter that is determined by the order of the value in the parameters
    /// ordered parameters are required to be passed before named parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterOrderedAttribute : Attribute
    {
        private string _helpName = "TODO";
        /// <summary>
        /// An unnamed parameter that is determined by the order of the value in the parameters
        /// ordered parameters are required to be passed before named parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public CommandParameterOrderedAttribute(string name, string description) 
        { 
            this.Name = name;
            this.ValueDescription = description;
        }

        /// <summary>
        /// even though this parameter does not reuqire a name, it is used for help
        /// </summary>
        public string Name
        {
            get { return _helpName; }
            set { _helpName = CommandDescription.GetValidCommandName(value, false); }
        }
        /// <summary>
        /// description of the value for help
        /// </summary>
        public string ValueDescription { get; set; } = "";
        /// <summary>
        /// input values that are allowed, anything else will throw an error
        /// case is ignored
        /// </summary>
        public string[] AllowedValues { get; set; } = [];
        /// <summary>
        /// used when no value is provided
        /// this satisfies the IsRequired flag
        /// </summary>
        public string DefaultValue { get; set; } = "";
        /// <summary>
        /// indicates if the parameter is required
        /// </summary>
        public bool IsRequired { get; set; } = true;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// </summary>
        public bool UsePipe { get; set; } = false;
        /// <summary>
        /// format the help string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var placeholder = $"<{_helpName}>";
            return $"{placeholder,18} {ValueDescription}".Trim();
        }
    }
}
