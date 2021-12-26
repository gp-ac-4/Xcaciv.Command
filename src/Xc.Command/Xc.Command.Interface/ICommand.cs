using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xc.Command.Interface
{
    public interface ICommand : IAsyncDisposable
    {
        /// <summary>
        /// name value collection of parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Task<string> Operate(Dictionary<string, string> parameters, IOutputMessageContext outputMesser);
    }
}
