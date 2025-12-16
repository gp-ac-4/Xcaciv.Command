using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// Defines the contract for a command that can be executed within the framework.
    /// All commands must implement this interface and be decorated with CommandRegisterAttribute.
    /// </summary>
    /// <remarks>
    /// Commands are discovered via attributes (CommandRegisterAttribute for registration,
    /// CommandRootAttribute for root-level sub-commands, and parameter attributes for
    /// parameter definitions). Commands support pipelining via IAsyncEnumerable output.
    /// 
    /// Security: Commands execute in isolated environments. Only commands with
    /// ModifiesEnvironment=true can alter environment variables.
    /// </remarks>
    public interface ICommandDelegate : IAsyncDisposable
    {
        /// <summary>
        /// Primary command execution method.
        /// </summary>
        /// <param name="ioContext">The IO context for input/output and parameter access.</param>
        /// <param name="env">The environment context for variable access (isolated child context if environment-modifying).</param>
        /// <returns>An async enumerable of output strings. Each string is output as a separate chunk.</returns>
        /// <remarks>
        /// Implementations should yield output via "yield return" or async enumeration.
        /// Output supports pipelining: if part of a piped command sequence, output is sent
        /// to the next command's input via channels.
        /// 
        /// The environment context passed is a child context isolated from the parent.
        /// Changes to this context are not reflected in the parent unless the command
        /// has ModifiesEnvironment=true on its CommandDescription.
        /// </remarks>
        IAsyncEnumerable<string> Main(IIoContext ioContext, IEnvironmentContext env);

        /// <summary>
        /// Outputs detailed usage instructions for the command.
        /// </summary>
        /// <param name="parameters">Command parameters (may include parameter names for context-specific help).</param>
        /// <param name="env">The environment context (for environment-dependent help text).</param>
        /// <returns>Multi-line help text describing command usage, parameters, and examples.</returns>
        /// <remarks>
        /// Called when user requests help via --HELP flag.
        /// Should include command syntax, parameter descriptions, and usage examples.
        /// Use AbstractCommand.BuildHelpString() for consistent formatting.
        /// </remarks>
        string Help(string[] parameters, IEnvironmentContext env);

        /// <summary>
        /// Outputs a single-line summary of the command's purpose.
        /// </summary>
        /// <param name="parameters">Command parameters (not typically used for one-line help).</param>
        /// <returns>A single line describing the command's purpose.</returns>
        /// <remarks>
        /// Used when listing all available commands. Should be brief (under 80 characters).
        /// Format typically: "COMMAND_NAME - Brief description of what it does"
        /// </remarks>
        string OneLineHelp(string[] parameters);
    }
}