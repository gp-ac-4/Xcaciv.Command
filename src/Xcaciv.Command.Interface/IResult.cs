namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// Represents the outcome of a command execution, including success state, payload, and formatting hints for consumers.
    /// </summary>
    /// <typeparam name="T">Type of the output payload returned by the command.</typeparam>
    public interface IResult<out T>
    {
        /// <summary>
        /// True when the command completed without errors; false when an error occurred.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// When <see cref="IsSuccess"/> is false, contains a human-readable description of the failure; otherwise null.
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// The exception captured during execution, if any. May be null when errors are represented only by <see cref="ErrorMessage"/>.
        /// </summary>
        Exception? Exception { get; }

        /// <summary>
        /// Correlation identifier used to trace this result across pipeline stages or logging systems.
        /// </summary>
        string CorrelationId { get; }

        /// <summary>
        /// The output payload when <see cref="IsSuccess"/> is true; may be null for commands that do not emit data.
        /// </summary>
        T? Output { get; }

        /// <summary>
        /// Indicates the format consumers should expect when rendering or serializing <see cref="Output"/>.
        /// </summary>
        ResultFormat OutputFormat { get; }
    }
}
