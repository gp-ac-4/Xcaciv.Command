namespace Xcaciv.Command;

/// <summary>
/// Backpressure policy when pipeline channel reaches capacity.
/// </summary>
public enum PipelineBackpressureMode
{
    /// <summary>
    /// Drop the oldest item when buffer is full (FIFO).
    /// </summary>
    DropOldest = 0,

    /// <summary>
    /// Reject the newest item when buffer is full (LIFO).
    /// </summary>
    DropNewest = 1,

    /// <summary>
    /// Block/wait for consumer to process items (backpressure).
    /// </summary>
    Block = 2
}
