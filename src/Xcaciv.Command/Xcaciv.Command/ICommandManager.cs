using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command
{
    public interface ICommandManager
    {
        void AddPackageDirectory(string directory);
        void LoadCommands(string subDirectory = "bin");
        Task Run(string commandLine, ITextIoContext output);
    }
}