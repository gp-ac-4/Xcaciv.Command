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
    public interface ICommandDelegate : IAsyncDisposable
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
        /// primary command execution method
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="messageContext">used for progress and status messages</param>
        /// <returns></returns>
        IAsyncEnumerable<string> Main(IInputContext input, IEnvironment statusContext);
        /// <summary>
        /// output usage instructions via message context
        /// </summary>
        /// <param name="messageContext"></param>
        /// <returns></returns>
        void Help(IOutputContext outputContext);
    }
}
 