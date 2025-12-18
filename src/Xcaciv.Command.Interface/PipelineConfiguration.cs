using System;

namespace Xcaciv.Command;

/// <summary>
/// Configuration for pipeline execution behavior, including channel capacity and backpressure policies.
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
    /// Timeout for pipeline execution in seconds.
    /// 0 = no timeout (unlimited execution time).
    /// </summary>
    public int ExecutionTimeoutSeconds { get; set; } = 0;
}
