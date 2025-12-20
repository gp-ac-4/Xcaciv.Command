using System.Threading;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface;

/// <summary>
/// Coordinates help handling, command instantiation, and execution lifecycle management.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Gets or sets the help command token (default "HELP").
    /// </summary>
    string HelpCommand { get; set; }

    /// <summary>
    /// Gets or sets the audit logger used for recording command execution events.
    /// </summary>
    IAuditLogger AuditLogger { get; set; }

    /// <summary>
    /// Executes the given command using the supplied context objects.
    /// </summary>
    Task ExecuteAsync(string commandKey, IIoContext ioContext, IEnvironmentContext environmentContext);

    /// <summary>
    /// Executes the given command with cancellation support.
    /// </summary>
    Task ExecuteAsync(string commandKey, IIoContext ioContext, IEnvironmentContext environmentContext, CancellationToken cancellationToken);

    /// <summary>
    /// Outputs help text for the requested command, or lists all commands when empty.
    /// </summary>
    Task GetHelpAsync(string command, IIoContext ioContext, IEnvironmentContext environmentContext);

    /// <summary>
    /// Outputs help text for the requested command with cancellation support, or lists all commands when empty.
    /// </summary>
    Task GetHelpAsync(string command, IIoContext ioContext, IEnvironmentContext environmentContext, CancellationToken cancellationToken);
}
