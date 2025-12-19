
namespace Xcaciv.Command.Interface
{
    public interface ICommandController
    {
        /// <summary>
        /// Register built-in commands in the command registry.
        /// This is the primary method for enabling the framework's default command set.
        /// </summary>
        void RegisterBuiltInCommands();
        
        /// <summary>
        /// [DEPRECATED] Use RegisterBuiltInCommands() instead.
        /// Provided for backward compatibility during migration to v2.0.
        /// Will be removed in v3.0.
        /// </summary>
        [Obsolete("Use RegisterBuiltInCommands() instead. This method will be removed in v3.0.", false)]
        void EnableDefaultCommands() => RegisterBuiltInCommands();
        
        /// <summary>
        /// add a directory from which to load commands
        /// </summary>
        /// <param name="directory"></param>
        void AddPackageDirectory(string directory);
        /// <summary>
        /// load commands from the pacakge driectory with a sub directory filter
        /// </summary>
        /// <param name="subDirectory"></param>
        void LoadCommands(string subDirectory = "bin");
        /// <summary>
        /// run a command using the loaded command pacakges
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        Task Run(string commandLine, IIoContext output, IEnvironmentContext env);
        /// <summary>
        /// run a command using the loaded command packages with cancellation support
        /// </summary>
        /// <param name="commandLine">Command line to execute</param>
        /// <param name="output">IO context</param>
        /// <param name="env">Environment context</param>
        /// <param name="cancellationToken">Cancellation token to cancel execution</param>
        Task Run(string commandLine, IIoContext output, IEnvironmentContext env, CancellationToken cancellationToken);
        
        /// <summary>
        /// Asynchronously get help information for a command.
        /// This is the primary method for retrieving command documentation.
        /// </summary>
        /// <param name="command">Command name or empty string for all commands</param>
        /// <param name="output">IO context for output</param>
        /// <param name="env">Environment context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task GetHelpAsync(string command, IIoContext output, IEnvironmentContext env, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// [DEPRECATED] Use GetHelpAsync() instead.
        /// Provided for backward compatibility during migration to v2.0.
        /// Will be removed in v3.0.
        /// </summary>
        [Obsolete("Use GetHelpAsync() instead. This method will be removed in v3.0.", false)]
        void GetHelp(string command, IIoContext output, IEnvironmentContext env);
        
        /// <summary>
        /// install a single command into the index
        /// </summary>
        /// <param name="command"></param>
        void AddCommand(ICommandDescription command);
        /// <summary>
        /// add a command from a loaded type
        /// good for  commands from internal or linked dlls
        /// </summary>
        /// <param name="packageKey"></param>
        /// <param name="commandType"></param>
        /// <param name="modifiesEnvironment"></param>
        void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false);
        /// <summary>
        /// add a command from an instance of the command
        /// good for commands from internal or linked dlls
        /// </summary>
        /// <param name="packageKey"></param>
        /// <param name="command"></param>
        /// <param name="modifiesEnvironment"></param>
        void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false);
    }
}