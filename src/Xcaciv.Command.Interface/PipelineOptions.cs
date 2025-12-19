namespace Xcaciv.Command.Interface;

/// <summary>
/// Configuration options for pipeline execution behavior.
/// </summary>
public class PipelineOptions
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json.
    /// </summary>
    public const string SectionName = "Xcaciv:Command:Pipeline";

    /// <summary>
    /// Maximum size of the channel queue between pipeline stages.
    /// Default: 100
    /// </summary>
    public int MaxChannelQueueSize { get; set; } = 100;

    /// <summary>
    /// Backpressure mode when the channel queue is full.
    /// Options: "Wait", "DropOldest", "DropNewest"
    /// Default: "Wait"
    /// </summary>
    public string BackpressureMode { get; set; } = "Wait";

    /// <summary>
    /// Converts the BackpressureMode string to the enum value.
    /// </summary>
    public PipelineBackpressureMode GetBackpressureMode()
    {
        return BackpressureMode.ToUpperInvariant() switch
        {
            "WAIT" or "BLOCK" => PipelineBackpressureMode.Block,
            "DROPOLDEST" => PipelineBackpressureMode.DropOldest,
            "DROPNEWEST" => PipelineBackpressureMode.DropNewest,
            _ => PipelineBackpressureMode.Block
        };
    }
}
