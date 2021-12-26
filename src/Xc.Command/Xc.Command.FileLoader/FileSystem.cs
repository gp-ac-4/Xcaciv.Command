namespace Xc.Command.FileLoader
{
    public class FileSystem
    {
        /// <summary>
        /// directories to search for command assemblies
        /// </summary>
        private static List<string> commandDirectories = new List<string>();
        /// <summary>
        /// get a copy of command direcory search list
        /// </summary>
        public static List<string> CommandDirectories => commandDirectories.ToList();

        /// <summary>
        /// restrict file path and translat to fully qualified file name
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="restrictedPath"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static bool VerifyRestrictedPath(string filePath, string? restrictedPath = null, bool shouldThrow = false)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));
            var fullFilePath = Path.GetDirectoryName(Path.GetFullPath(filePath));

            if (String.IsNullOrEmpty(restrictedPath)) restrictedPath = Directory.GetCurrentDirectory();
            var fullRestrictedPath = Path.GetFullPath(restrictedPath);

            if (String.IsNullOrEmpty(fullFilePath) || !(new Uri(fullRestrictedPath)).IsBaseOf(new Uri(fullFilePath)))
            {
                if (shouldThrow) throw new ArgumentOutOfRangeException(nameof(filePath) + " must be located within " + nameof(restrictedPath));
                else return false;
            }            

            return true;
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
            if (VerifyRestrictedPath(filePath, restrictedPath, shouldThrow) && File.Exists(filePath))
            {
                return !File.GetAttributes(filePath)
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
                return File.GetAttributes(filePath)
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
