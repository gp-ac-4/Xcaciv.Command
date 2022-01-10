using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xc.Command.Interface;

namespace Xc.Command
{
    /// <summary>
    /// Command Manager
    /// </summary>
    public class Manager
    {
        protected Dictionary<string, CommandDescription> Commands { get; set; } =  new Dictionary<string, CommandDescription>();
    }
}
