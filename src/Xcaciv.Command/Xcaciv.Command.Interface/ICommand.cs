using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// interface for commands that can be issued from a shell
    /// </summary>
    public interface ICommand : IAsyncDisposable
    {
        /// <summary>
        /// unique typed command - alphanumeric with dash or underscore
        /// </summary>
        string BaseCommand { get; }
        /// <summary>
        /// Display name - may contain special characters
        /// </summary>
        string FriendlyName { get; }
        /// <summary>
        /// name value collection of parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        Task<string> Main(string[] parameters, ITextIoContext messageContext);
        /// <summary>
        /// output usage instructions via message context
        /// </summary>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        Task Help(ITextIoContext messageContext);
    }
}
 