
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
        Task Run(string commandLine, ITextIoContext output);
        /// <summary>
        /// get provided help string for a command or list all commands if the command is empty
        /// </summary>
        /// <param name="command"></param>
        /// <param name="output"></param>
        void GetHelp(string command, IOutputContext output);
    }
}