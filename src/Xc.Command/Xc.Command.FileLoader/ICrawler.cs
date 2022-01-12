using Xc.Command.Interface;

namespace Xc.Command.FileLoader
{
    public interface ICrawler
    {
        IDictionary<string, PackageDescription> LoadPackageDescriptions(string basePath, string subDirectory);
    }
}