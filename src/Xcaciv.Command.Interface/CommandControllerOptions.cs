namespace Xcaciv.Command.Interface;

/// <summary>
/// Configuration options for the command controller.
/// Can be bound from appsettings.json or other configuration sources.
/// </summary>
public class CommandControllerOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json.
    /// </summary>
    public const string SectionName = "Xcaciv:Command";

    /// <summary>
    /// The command keyword that triggers help display.
    /// Default: "HELP"
    /// </summary>
    public string HelpCommand { get; set; } = "HELP";

    /// <summary>
    /// Whether to enable default built-in commands on startup.
    /// Default: true
    /// </summary>
    public bool EnableDefaultCommands { get; set; } = true;

    /// <summary>
    /// Package directories to scan for plugin commands on startup.
    /// </summary>
    public string[] PackageDirectories { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Base directory restriction for plugin loading security.
    /// If set, plugins can only be loaded from subdirectories of this path.
    /// </summary>
    public string? RestrictedDirectory { get; set; }

    /// <summary>
    /// Enable verbose logging for troubleshooting.
    /// Default: false
    /// </summary>
    public bool Verbose { get; set; } = false;
}
