namespace Xcaciv.Command.Interface
{
    public interface ICommandDescription
    {
        /// <summary>
        /// string used to identify the command
        /// </summary>
        string BaseCommand { get; }
        /// <summary>
        /// subcommnands that are registered in their own classes
        /// </summary>
        Dictionary<string, ICommandDescription> SubCommands { get; }
        /// <summary>
        /// base command type name
        /// type must implement ICommandDelegate
        /// </summary>
        string FullTypeName { get; }
        /// <summary>
        /// indicates the intent to modify the environment values
        /// this is only allowed for certain commands
        /// </summary>
        bool ModifiesEnvironment { get; }
        /// <summary>
        /// assembly information for loading the command
        /// </summary>
        PackageDescription PackageDescription { get; }

        /// <summary>
        /// parse primary command from a command line
        /// </summary>
        /// <param name="commandLine">full command line</param>
        /// <param name="upper">convert to uppercase</param>
        /// <returns>validated command name</returns>
        static string GetValidCommandName(string commandLine, bool upper = true)
        {
            commandLine = commandLine.Trim();
            var commandText = (commandLine.Contains(' ') ?
                    commandLine.Substring(0, commandLine.Trim().IndexOf(' '))
                     : commandLine).Trim('-');
            // remove invalid characters - only allow alphanumeric, dashes, underscores, and spaces
            commandText = System.Text.RegularExpressions.Regex.Replace(commandText.Trim(), @"[^-_\da-zA-Z ]+", "");
            // set proper case
            return (upper) ?
                commandText.ToUpper() :
                commandText.ToLower();
        }

        /// <summary>
        /// parses arguments from a command line
        /// </summary>
        /// <param name="commandLine">full command line</param>
        /// <returns>array of arguments (excluding command name)</returns>
        static string[] GetArgumentsFromCommandline(string commandLine)
        {
            var args = System.Text.RegularExpressions.Regex.Matches(commandLine, @"[\""].*?[\""]|[\w-]+")
                .Cast<System.Text.RegularExpressions.Match>()
                .Select(o => System.Text.RegularExpressions.Regex.Replace(o.Value, @"[^-_\da-zA-Z .*?\[\]|""~!@#$%^&*\(\)]+", "").Trim('"'))
                .ToArray();

            // the first item in the array is the command
            return args.Skip(1).ToArray();
        }
    }
}