using System.IO.Abstractions;

namespace Xc.Command.FileLoader
{
    public static class VerfiedSourceDirectories
    {
        public static IFileSystem FileSystem { get; set; } = new FileSystem();
        /// <summary>
        /// directories to search for command assemblies
        /// </summary>
        private static List<string> commandDirectories = new List<string>();
        /// <summary>
        /// get a copy of command direcory search list
        /// </summary>
        public static IReadOnlyList<string> Directories => commandDirectories;
        public static bool VerifyRestrictedPath(IPath pathWrapper,string filePath, string? restrictedPath = null, bool shouldThrow = false)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            var fullFilePath = pathWrapper.GetDirectoryName(pathWrapper.GetFullPath(filePath));

            if (String.IsNullOrEmpty(restrictedPath)) restrictedPath = Directory.GetCurrentDirectory();
            var fullRestrictedPath = pathWrapper.GetFullPath(restrictedPath);

            if (String.IsNullOrEmpty(fullFilePath) || !(new Uri(fullRestrictedPath)).IsBaseOf(new Uri(fullFilePath)))
            {
                if (shouldThrow) throw new ArgumentOutOfRangeException(nameof(filePath) + " must be located within " + nameof(restrictedPath));
                else return false;
            }

            return true;
        }
        /// <summary>
        /// restrict file path and translat to fully qualified file name
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="restrictedPath"></param>
        /// <param name="shouldThrow"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool VerifyRestrictedPath(string filePath, string? restrictedPath = null, bool shouldThrow = false)
        {
            var pathWrapper = new PathWrapper(FileSystem);
            return VerifyRestrictedPath(pathWrapper, filePath, restrictedPath, shouldThrow);
            
        }
        /// <summary>
        /// confirm a file exists within the restricted path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="restrictedPath"></param>
        /// <param name="shouldThrow"></param>
        /// <returns></returns>
        public static bool VerifyFile(string filePath, string? restrictedPath = null, bool shouldThrow = false)
        {
            if (VerifyRestrictedPath(filePath, restrictedPath, shouldThrow) && FileSystem.File.Exists(filePath))
            {
                return !FileSystem.File.GetAttributes(filePath)
                    .HasFlag(FileAttributes.Directory);
            }

            return false;
        }
        /// <summary>
        /// confirm a directory exists within a restricted path
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="restrictedPath"></param>
        /// <param name="shouldThrow"></param>
        /// <returns></returns>
        public static bool VerifyDirectory(string filePath, string? restrictedPath = null, bool shouldThrow = false)
        {
            if(VerifyRestrictedPath(filePath, restrictedPath, shouldThrow) && Directory.Exists(filePath))
            {
                return FileSystem.File.GetAttributes(filePath)
                    .HasFlag(FileAttributes.Directory);
            }

            return false;
        }
        /// <summary>
        /// add to the command directory search list
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="restrictedPath"></param>
        /// <returns></returns>
        public static bool AddCommandDirectory(string directory, string? restrictedPath = null)
        {
            if (!VerifyDirectory(directory, restrictedPath)) return false;

            commandDirectories.Add(directory);

            return true;
        }
    }
}
