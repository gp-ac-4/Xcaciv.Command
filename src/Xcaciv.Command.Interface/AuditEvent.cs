using System;
using System.Collections.Generic;

namespace Xcaciv.Command.Interface;

/// <summary>
/// Structured audit event for command execution.
/// Provides rich metadata for compliance, monitoring, and troubleshooting.
/// </summary>
public sealed class AuditEvent
{
    /// <summary>
    /// Unique correlation identifier for this command invocation.
    /// Used to track a command through its lifecycle and across pipeline stages.
    /// </summary>
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the command being executed.
    /// </summary>
    public required string CommandName { get; init; }

    /// <summary>
    /// Package or assembly origin of the command (e.g., "built-in", "MyPlugin.dll").
    /// </summary>
    public string? PackageOrigin { get; init; }

    /// <summary>
    /// Command parameters, potentially redacted based on masking rules.
    /// </summary>
    public required string[] Parameters { get; init; }

    /// <summary>
    /// UTC timestamp when command execution started.
    /// </summary>
    public DateTime ExecutedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of command execution.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Whether the command completed successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if the command failed, otherwise null.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Stage in a pipeline (e.g., 1, 2, 3) if this command is part of a pipeline.
    /// Null for standalone command execution.
    /// </summary>
    public int? PipelineStage { get; init; }

    /// <summary>
    /// Total number of stages in the pipeline, if applicable.
    /// </summary>
    public int? PipelineTotalStages { get; init; }

    /// <summary>
    /// Additional context metadata for this execution.
    /// </summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
