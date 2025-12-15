using System;

namespace Xcaciv.Command.Interface;

/// <summary>
/// Interface for audit logging of command execution and environment changes.
/// Provides structured audit trail for compliance and troubleshooting.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Log command execution with parameters and result.
    /// </summary>
    /// <param name="commandName">Name of the command executed</param>
    /// <param name="parameters">Command parameters (sanitized)</param>
    /// <param name="executedAt">UTC timestamp when command started</param>
    /// <param name="duration">Time taken to execute</param>
    /// <param name="success">Whether the command completed successfully</param>
    /// <param name="errorMessage">Error message if command failed</param>
    void LogCommandExecution(
        string commandName,
        string[] parameters,
        DateTime executedAt,
        TimeSpan duration,
        bool success,
        string? errorMessage = null);

    /// <summary>
    /// Log environment variable changes.
    /// </summary>
    /// <param name="variableName">Name of the environment variable</param>
    /// <param name="oldValue">Previous value (null if new variable)</param>
    /// <param name="newValue">New value being set</param>
    /// <param name="changedBy">Who/what initiated the change</param>
    /// <param name="changedAt">UTC timestamp of the change</param>
    void LogEnvironmentChange(
        string variableName,
        string? oldValue,
        string? newValue,
        string changedBy,
        DateTime changedAt);
}
