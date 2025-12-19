using System;
using System.Text.Json;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command;

/// <summary>
/// Structured audit logger that outputs JSON-formatted audit events to console.
/// Suitable for production use with log aggregation systems.
/// </summary>
public class StructuredAuditLogger : IAuditLogger
{
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Gets or sets the masking configuration for parameter redaction.
    /// </summary>
    public AuditMaskingConfiguration? MaskingConfiguration { get; set; } = new AuditMaskingConfiguration();

    /// <summary>
    /// Log command execution using legacy method signature.
    /// Converts to structured AuditEvent internally.
    /// </summary>
    public void LogCommandExecution(
        string commandName,
        string[] parameters,
        DateTime executedAt,
        TimeSpan duration,
        bool success,
        string? errorMessage = null)
    {
        var maskedParams = MaskingConfiguration?.ApplyMasking(parameters) ?? parameters;

        var auditEvent = new AuditEvent
        {
            CommandName = commandName,
            Parameters = maskedParams,
            ExecutedAt = executedAt,
            Duration = duration,
            Success = success,
            ErrorMessage = errorMessage
        };

        LogAuditEvent(auditEvent);
    }

    /// <summary>
    /// Log environment variable changes.
    /// </summary>
    public void LogEnvironmentChange(
        string variableName,
        string? oldValue,
        string? newValue,
        string changedBy,
        DateTime changedAt)
    {
        var envChange = new
        {
            EventType = "EnvironmentChange",
            Timestamp = changedAt,
            VariableName = variableName,
            OldValue = ShouldMaskEnvironmentVariable(variableName) ? "[REDACTED]" : oldValue,
            NewValue = ShouldMaskEnvironmentVariable(variableName) ? "[REDACTED]" : newValue,
            ChangedBy = changedBy
        };

        var json = JsonSerializer.Serialize(envChange, _jsonOptions);
        Console.WriteLine(json);
    }

    /// <summary>
    /// Log a structured audit event with full metadata.
    /// </summary>
    public void LogAuditEvent(AuditEvent auditEvent)
    {
        if (auditEvent == null)
            throw new ArgumentNullException(nameof(auditEvent));

        // Apply masking if not already applied
        var maskedParams = MaskingConfiguration?.ApplyMasking(auditEvent.Parameters) ?? auditEvent.Parameters;

        var logEntry = new
        {
            EventType = "CommandExecution",
            auditEvent.CorrelationId,
            auditEvent.CommandName,
            auditEvent.PackageOrigin,
            Parameters = maskedParams,
            Timestamp = auditEvent.ExecutedAt,
            DurationMs = auditEvent.Duration.TotalMilliseconds,
            auditEvent.Success,
            auditEvent.ErrorMessage,
            auditEvent.PipelineStage,
            auditEvent.PipelineTotalStages,
            auditEvent.Metadata
        };

        var json = JsonSerializer.Serialize(logEntry, _jsonOptions);
        Console.WriteLine(json);
    }

    private bool ShouldMaskEnvironmentVariable(string variableName)
    {
        if (MaskingConfiguration == null)
            return false;

        return MaskingConfiguration.ShouldRedact(variableName);
    }
}
