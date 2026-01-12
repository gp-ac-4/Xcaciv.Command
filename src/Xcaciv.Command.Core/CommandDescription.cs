using System.Text.RegularExpressions;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command;

/// <summary>
/// Information about a command and how to execute it
/// </summary>
public class CommandDescription : ICommandDescription
{
    /// <summary>
    /// sanitized internal command text
    /// </summary>
    protected string command = string.Empty;
    /// <summary>
    /// regex for clensing command
    /// no exceptions
    /// </summary>
    public static Regex InvalidCommandChars = new Regex(@"[^-_\da-zA-Z ]+");
    /// <summary>
    /// regex for clensing parameters
    /// no exceptions
    /// </summary>
    public static Regex InvalidParameterChars = new Regex(@"[^-_\da-zA-Z .*?\[\]|""~!@#$%^&*\(\)]+");
    /// <summary>
    /// text command
    /// </summary>
    public string BaseCommand { get => command; set => command = GetValidCommandName(value); }
    /// <summary>
    /// sub command text
    /// used to limit the secondary command text
    /// </summary>
    public Dictionary<string, ICommandDescription> SubCommands { get; set; } = new Dictionary<string, ICommandDescription>(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// Fully Namespaced Type Name
    /// </summary>
    public string FullTypeName { get; set; } = "";
    /// <summary>
    /// full path to containing assembly
    /// </summary>
    public PackageDescription PackageDescription { get; set; } = new PackageDescription();
    /// <summary>
    /// explicitly indicates if a command modifes the environment
    /// </summary>
    public bool ModifiesEnvironment { get; set; }

    /// <summary>
    /// parse primary command from a command line
    /// </summary>
    /// <param name="commandLine">full command line</param>
    /// <param name="upper">convert to uppercase</param>
    /// <returns></returns>
    public static string GetValidCommandName(string commandLine, bool upper = true)
    {
        commandLine = commandLine.Trim();
        var commandText = (commandLine.Contains(' ') ?
                commandLine.Substring(0, commandLine.Trim().IndexOf(' '))
                 : commandLine).Trim('-');
        // remove invalid characters
        commandText = InvalidCommandChars.Replace(commandText.Trim(), "");
        // set proper case
        return (upper) ?
            commandText.ToUpper() :
            commandText.ToLower();
    }
    /// <summary>
    /// parses arguments from a command line
    /// </summary>
    /// <param name="commandLine">full command line</param>
    /// <returns></returns>
    public static string[] GetArgumentsFromCommandline(string commandLine)
    {
        var args = Regex.Matches(commandLine, @"[\""].*?[\""]|[\w-]+")
            .Cast<Match>()
            .Select(o => InvalidParameterChars.Replace(o.Value, "").Trim('"'))
            .ToArray();

        // the first item in the array is the command
        return args.Skip(1).ToArray();
    }
}
