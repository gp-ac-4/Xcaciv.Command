using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BaseCommandAttribute : Attribute
    {
        private string _command;
        /// <summary>
        /// define how this command is to be called
        /// </summary>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public BaseCommandAttribute(string command = "", string description = "") 
        { 
            this._command = command.ToUpper();
            this.Description = description;
        }
        /// <summary>
        /// the base command string
        /// </summary>
        /// <example>DIR</example>
        public string Command { 
            get
            { return this._command; }
            set
            { this._command = value.ToUpper(); }
        }
        /// <summary>
        /// What does this command do
        /// </summary>
        public string Description { get; set; } = "TODO";
        /// <summary>
        /// Prototype/example of how to call the command with parameters parameters
        /// </summary>
        /// <example>CMD [-A | -U] [-Q] [-D] [-E ON | OFF]</example>
        public string Prototype { get; set; } = "TODO";
    }
}
