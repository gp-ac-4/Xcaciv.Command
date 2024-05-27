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
        /// indicates if the parameter is required
        /// </summary>
        public bool IsRquired { get; set; } = true;
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
