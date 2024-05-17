
namespace Xcaciv.Command.Interface
{
    public interface ICommandController
    {
        void AddPackageDirectory(string directory);
        void LoadCommands(string subDirectory = "bin");
        Task Run(string commandLine, ITextIoContext output);
    }
}