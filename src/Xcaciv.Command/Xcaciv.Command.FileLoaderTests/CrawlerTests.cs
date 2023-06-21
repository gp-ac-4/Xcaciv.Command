using Xcaciv.Command.FileLoader;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using System.IO;

namespace Xcaciv.Command.FileLoaderTests;

public class CrawlerTests
{
    private static string basePath = @"C:\Program\Commands\";
    private static string subDirectory = "bin";

    private IFileSystem getFileSystem()
    {
        var filesSystem = new MockFileSystem(new Dictionary<string, MockFileData>() {
                {$@"{basePath}Stuff\{subDirectory}readme.txt", new MockFileData("Testing is meh.") },
                {$@"{basePath}Hello\{subDirectory}\Hello.dll", new MockFileData(Resource1.zTestAssembly) },
                {$@"{basePath}Hello\{subDirectory}\zTestInterfaces.dll", new MockFileData(Resource1.zTestInterfaces) },
                {$@"{basePath}Say\readme.txt", new MockFileData("Testing is meh.") },
                {$@"{basePath}Do\readme.txt", new MockFileData("Testing is meh.") },
                {$@"{basePath}long\deep\{subDirectory}readme.txt", new MockFileData("Testing is meh.") },
                {$@"{basePath}no\return\{subDirectory}\Hello.dll", new MockFileData(Resource1.zTestAssembly) },
                {$@"{basePath}too\deep\{subDirectory}\zTestInterfaces.dll", new MockFileData(Resource1.zTestInterfaces) },
                {$@"{basePath}Root\Hello\{subDirectory}\RootHello.dll", new MockFileData(Resource1.zTestAssembly) },
                {$@"{basePath}Root\Hello\{subDirectory}\zTestInterfaces.dll", new MockFileData(Resource1.zTestInterfaces) },
                {$@"{basePath}Xc.Hello\{subDirectory}\Xc.Hello.dll", new MockFileData(Resource1.zTestAssembly) },
            });
        filesSystem.AddDirectory(@"C:\Program\Commands\Hello");
        filesSystem.AddDirectory(@"C:\Program\Commands\Say");
        filesSystem.AddDirectory(@"C:\Program\Commands\Do");
        filesSystem.AddDirectory(@"C:\Program\Commands\Stuff");

        return filesSystem;
    }
    /*
            [Fact()]
            public void WalkPackagePathsTest()
            {
                var fileSystem = this.getFileSystem();
                var crawler = new Crawler(fileSystem);

                var firstBin = crawler.LoadCommandDescriptions(basePath, "bin").FirstOrDefault();
                Assert.Equal("C:\\Program\\Commands\\Hello\\bin\\Hello.dll", firstBin.Value.AssemblyPath);
            }
    */

    [Fact()]
    public void WalkPackagePathsTest1()
    {
        var fileSystem = this.getFileSystem();
        var crawler = new Crawler(fileSystem);

        var paths = new Dictionary<string, string>();
        crawler.CrawlPackagePaths(basePath, subDirectory, (name, binPath) => paths.Add(name, binPath));

        Assert.Equal("C:\\Program\\Commands\\Hello\\bin\\Hello.dll", paths.FirstOrDefault().Value);
    }
}
