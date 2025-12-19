
namespace Xcaciv.Command.Interface
{
    public interface ICommandController
    {
        /// <summary>
        /// enable compiled-in commands
        /// </summary>
        void EnableDefaultCommands();
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
        /// get provided help string for a command or list all commands if the command is empty
        /// </summary>
        /// <param name="command"></param>
        /// <param name="output"></param>
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