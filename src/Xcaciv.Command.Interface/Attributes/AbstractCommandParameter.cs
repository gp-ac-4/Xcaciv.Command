using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    public abstract class AbstractCommandParameter : Attribute
    {
        protected string _helpName = "TODO";
        /// <summary>
        /// description of the value for help
        /// </summary>
        public string ValueDescription { get; set; } = "";
        /// <summary>
        /// even though this parameter does not reuqire a name, it is used for help
        /// </summary>
        public string Name
        {
            get { return _helpName; }
            set { _helpName = CommandDescription.GetValidCommandName(value, false); }
        }
        /// <summary>
        /// format the help string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var placeholder = $"<{_helpName}>";
            return $"{placeholder,-18} {ValueDescription}".Trim();
        }
    }
}
