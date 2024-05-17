using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command
{
    public interface ICommandController
    {
        void AddPackageDirectory(string directory);
        void LoadCommands(string subDirectory = "bin");
        Task Run(string commandLine, ITextIoContext output);
        void LoadDefaultCommands();
        void AddCommand(CommandDescription command);
        void GetHelp(string command, IOutputContext context);
    }
}