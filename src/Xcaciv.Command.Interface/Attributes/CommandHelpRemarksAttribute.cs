using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandHelpRemarksAttribute : Attribute
    {
        /// <summary>
        /// add remarks to help output
        /// </summary>
        /// <param name="remarks"></param>
        public CommandHelpRemarksAttribute(string remarks) 
        {
            this.Remarks = remarks;
        }
        /// <summary>
        /// further explination to be displayed in help
        /// </summary>
        public string Remarks { get; set; }
    }
}
