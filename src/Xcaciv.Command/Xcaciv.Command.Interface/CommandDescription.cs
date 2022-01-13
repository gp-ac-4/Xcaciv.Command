using System.Text.RegularExpressions;

namespace Xcaciv.Command.Interface;

/// <summary>
/// Information about a command and how to execute it
/// </summary>
public class CommandDescription
{
    /// <summary>
    /// sanitized internal command text
    /// </summary>
    protected string command = string.Empty;
    /// <summary>
    /// regex for clensing command
    /// no exceptions
    /// </summary>
    public static Regex InvalidCommandChars = new Regex(@"[^-_\da-zA-Z ""]+");
    /// <summary>
    /// text command
    /// </summary>
    public string BaseCommand { get => command; set => command = InvalidCommandChars.Replace(value, "").ToUpper(); }
    /// <summary>
    /// Fully Namespaced Type Name
    /// </summary>
    public string FullTypeName { get; set; } = "";
    /// <summary>
    /// full path to containing assembly
    /// </summary>
    public PackageDescription PackageDescription { get; set; } = new PackageDescription();
}
