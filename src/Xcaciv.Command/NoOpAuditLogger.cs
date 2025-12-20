using System;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command;

/// <summary>
/// Default no-operation audit logger that discards all logs.
/// Used when no explicit audit logger is configured.
/// </summary>
public class NoOpAuditLogger : IAuditLogger
{
    /// <summary>
    /// Log command execution (no-op).
    /// </summary>
    public void LogCommandExecution(
        string commandName,
        string[] parameters,
        DateTime executedAt,
        TimeSpan duration,
        bool success,
        string? errorMessage = null)
    {
        // No operation - logs are discarded
    }

    /// <summary>
    /// Log environment change (no-op).
    /// </summary>
    public void LogEnvironmentChange(
        string variableName,
        string? oldValue,
        string? newValue,
        string changedBy,
        DateTime changedAt)
    {
        // No operation - logs are discarded
    }

    /// <summary>
    /// Log structured audit event (no-op).
    /// </summary>
    public void LogAuditEvent(AuditEvent auditEvent)
    {
        // No operation - logs are discarded
    }

    /// <summary>
    /// Gets or sets the masking configuration (no-op).
    /// </summary>
    public AuditMaskingConfiguration? MaskingConfiguration { get; set; }
}
