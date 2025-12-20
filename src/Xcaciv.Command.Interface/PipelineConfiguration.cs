using System;

namespace Xcaciv.Command;

/// <summary>
/// Configuration for pipeline execution behavior, including channel capacity, backpressure policies,
/// and per-stage resource limits for DoS protection.
/// </summary>
public class PipelineConfiguration
{
    /// <summary>
    /// Maximum number of items allowed in pipeline channels.
    /// Default: 10,000 items (approximately 100MB for 10KB strings).
    /// </summary>
    public int MaxChannelQueueSize { get; set; } = 10_000;

    /// <summary>
    /// Policy when channel buffer is full.
    /// - DropOldest: Remove oldest item, add new item (FIFO drop)
    /// - DropNewest: Reject new item, keep old items (LIFO drop)
    /// - Block: Wait for consumer to make room (backpressure) default.
    /// </summary>
    public PipelineBackpressureMode BackpressureMode { get; set; } = PipelineBackpressureMode.Block;

    /// <summary>
    /// Timeout for entire pipeline execution in seconds.
    /// 0 = no timeout (unlimited execution time).
    /// </summary>
    public int ExecutionTimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Timeout for individual pipeline stage execution in seconds.
    /// 0 = no per-stage timeout (unlimited per stage).
    /// Applied to each command in the pipeline independently.
    /// </summary>
    public int StageTimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Maximum total output bytes allowed from a single pipeline stage.
    /// 0 = no limit (unlimited bytes).
    /// When exceeded, stage is cancelled with an error.
    /// </summary>
    public long MaxStageOutputBytes { get; set; } = 0;

    /// <summary>
    /// Maximum number of items allowed to flow through a single stage.
    /// 0 = no limit (unlimited items).
    /// When exceeded, stage is cancelled with an error.
    /// </summary>
    public int MaxStageOutputItems { get; set; } = 0;

    /// <summary>
    /// Validate configuration and throw if values are invalid.
    /// </summary>
    public void Validate()
    {
        if (MaxChannelQueueSize <= 0)
            throw new InvalidOperationException("MaxChannelQueueSize must be positive");
        
        if (ExecutionTimeoutSeconds < 0)
            throw new InvalidOperationException("ExecutionTimeoutSeconds cannot be negative");
        
        if (StageTimeoutSeconds < 0)
            throw new InvalidOperationException("StageTimeoutSeconds cannot be negative");
        
        if (MaxStageOutputBytes < 0)
            throw new InvalidOperationException("MaxStageOutputBytes cannot be negative");
        
        if (MaxStageOutputItems < 0)
            throw new InvalidOperationException("MaxStageOutputItems cannot be negative");
    }
}
