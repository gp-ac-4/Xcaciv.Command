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
    /// <returns>validated command name</returns>
    public static string GetValidCommandName(string commandLine, bool upper = true)
    {
        return CommandNameValidator.GetValidCommandName(commandLine, upper);
    }
    
    /// <summary>
    /// parses arguments from a command line
    /// </summary>
    /// <param name="commandLine">full command line</param>
    /// <returns>array of arguments (excluding command name)</returns>
    public static string[] GetArgumentsFromCommandline(string commandLine)
    {
        return CommandNameValidator.GetArgumentsFromCommandline(commandLine);
    }
}
