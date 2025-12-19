namespace Xcaciv.Command.Interface;

/// <summary>
/// Service for building help output from command metadata.
/// Centralizes help formatting so commands supply metadata instead of rendering help strings directly.
/// </summary>
public interface IHelpService
{
    /// <summary>
    /// Build full help output for a command instance using reflection on its attributes.
    /// </summary>
    /// <param name="command">The command instance to extract metadata from.</param>
    /// <param name="parameters">Command parameters for context.</param>
    /// <param name="environment">Environment context for conditional help.</param>
    /// <returns>Formatted help string.</returns>
    string BuildHelp(ICommandDelegate command, string[] parameters, IEnvironmentContext environment);

    /// <summary>
    /// Build single-line help for listing commands using command description.
    /// </summary>
    /// <param name="commandDescription">The command description containing base metadata.</param>
    /// <returns>Single-line formatted help string.</returns>
    string BuildOneLineHelp(ICommandDescription commandDescription);

    /// <summary>
    /// Check if parameters contain a help request (e.g., --HELP).
    /// </summary>
    /// <param name="parameters">Parameters to check.</param>
    /// <returns>True if help was requested.</returns>
    bool IsHelpRequest(string[] parameters);
}
