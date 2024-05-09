using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Loader;

namespace Xcaciv.Command.FileLoader;

public class Crawler : ICrawler
{
    /// <summary>
    /// number of package dlls signaling the need to paralell process
    /// </summary>
    public static int ParallelizeAt { get; set; } = 50;

    private const string SearchPattern = "*.dll";

    /// <summary>
    /// abstraction for file system
    /// </summary>
    protected IFileSystem fileSystem;
    /// <summary>
    /// empty constructor with default IFileSystem
    /// </summary>
    public Crawler() : this(new FileSystem()) { }
    /// <summary>
    /// constructor for testing with test IFileSystem
    /// </summary>
    /// <param name="fileSystem"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Crawler(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }
    /// <summary>
    /// interigate packages for commands and return descriptions
    /// </summary>
    /// <param name="basePath"></param>
    /// <param name="subDirectory"></param>
    /// <returns></returns>
    public IDictionary<string, PackageDescription> LoadPackageDescriptions(string basePath, string subDirectory)
    {
        var packages = new ConcurrentDictionary<string, PackageDescription>();

        // use a callback to process listing commands
        this.CrawlPackagePaths(basePath, subDirectory, (key, binPath) =>
        {
            // This is the package action

            var packagDesc = new PackageDescription()
            {
                Name = key,
                FullPath = binPath,
            };

            using (var context = AssemblyContext.LoadFromPath(binPath))
            {
                var commands = new Dictionary<string, CommandDescription>();
                packagDesc.Version = context.GetVersion();

                foreach (var command in context.GetAllInstances<ICommandDelegate>())
                {
                    commands[command.BaseCommand] = new CommandDescription()
                    {
                        BaseCommand = command.BaseCommand,
                        FullTypeName = command.GetType().FullName ?? String.Empty,
                        PackageDescription = packagDesc
                    };
                }

                packagDesc.Commands = commands;
            }

            if (packagDesc.Commands.Count > 0) packages.TryAdd(key, packagDesc);
        });

        return packages;
    }

    /// <summary>
    /// walk a set of directories matching a path convention
    /// NOTE: can perform paralell processing if number of directories 
    /// </summary>
    /// <param name="basePath"></param>
    /// <param name="subDirectory"></param>
    /// <param name="packageAction">Action(name, path) THREAD SAFE</param>
    public void CrawlPackagePaths(string basePath, string subDirectory, Action<string, string> packageAction)
    {
        basePath = fileSystem.Path.GetFullPath(basePath);
        if (!this.fileSystem.Directory.Exists(basePath)) throw new DirectoryNotFoundException(basePath);

        string searchMask = (String.IsNullOrEmpty(subDirectory)) ? SearchPattern : fileSystem.Path.Combine("*", subDirectory, SearchPattern);

        var binaryCommandCollections = this.fileSystem.Directory.GetFiles(basePath, searchMask, SearchOption.AllDirectories);

        if (!binaryCommandCollections.Any()) throw new Exceptions.NoPackageDirectoryFoundException($"No packages found in {basePath}.");

        // avoid overhead of paralell if it is not needed
        if (binaryCommandCollections.Count() > ParallelizeAt)
        {
            ForEachDirectoryParallel(basePath, subDirectory, packageAction, binaryCommandCollections);
        }
        else
        {
            ForEachDirectory(basePath, subDirectory, packageAction, binaryCommandCollections);
        }
    }
    /// <summary>
    /// liniar direcory processing using supplied action
    /// </summary>
    /// <param name="basePath"></param>
    /// <param name="subDirectory"></param>
    /// <param name="packageAction"></param>
    /// <param name="binaryDirectories"></param>
    protected void ForEachDirectory(string basePath, string subDirectory, Action<string, string> packageAction, string[] binaryDirectories)
    {
        foreach (var packageFilePath in binaryDirectories)
        {
            var fileName = fileSystem.Path.GetFileNameWithoutExtension(packageFilePath);
            var uniqueId = fileSystem.Path.GetDirectoryName(packageFilePath)?.Remove(0, basePath.Length)?.Replace(@"\", String.Empty);
            var packageName = $"{fileName}-{uniqueId}";
            if (fileSystem.File.Exists(packageFilePath)) packageAction(packageName, packageFilePath);
        }
    }
    /// <summary>
    /// parallel direcory processing using supplied action
    /// </summary>
    /// <param name="basePath"></param>
    /// <param name="subDirectory"></param>
    /// <param name="packageAction"></param>
    /// <param name="binaryDirectories"></param>
    protected void ForEachDirectoryParallel(string basePath, string subDirectory, Action<string, string> packageAction, string[] binaryDirectories)
    {
        Parallel.ForEach(binaryDirectories, (packageFilePath) =>
        {
            var fileName = fileSystem.Path.GetFileNameWithoutExtension(packageFilePath);
            var uniqueId = fileSystem.Path.GetDirectoryName(packageFilePath)?.Remove(0, basePath.Length)?.Replace(@"\", String.Empty);
            var packageName = $"{fileName}-{uniqueId}";
            if (fileSystem.File.Exists(packageFilePath)) packageAction(packageName, packageFilePath);
        });
    }
}
