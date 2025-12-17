using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// Defines an interface for encoding command output for safe consumption by different target systems.
    /// </summary>
    /// <remarks>
    /// Output encoders allow command output to be safely formatted for web UIs, log aggregators, 
    /// JSON APIs, and other systems that have specific escaping/encoding requirements.
    /// 
    /// The encoder is applied to all output chunks via IIoContext.OutputChunk() if configured.
    /// By default, commands use NoOpEncoder which performs no transformation (fully backward compatible).
    /// </remarks>
    public interface IOutputEncoder
    {
        /// <summary>
        /// Encodes output for safe consumption by the target system.
        /// </summary>
        /// <param name="output">The raw output string from a command.</param>
        /// <returns>The encoded output string, safe for the target system.</returns>
        /// <remarks>
        /// Implementations should be idempotent and thread-safe.
        /// Examples:
        /// - HtmlEncoder: HTML-escapes special characters (&lt;, &gt;, &amp;, etc.)
        /// - JsonEncoder: JSON-escapes special characters (quotes, backslashes, control chars)
        /// - NoOpEncoder: Returns input unchanged (default)
        /// </remarks>
        string Encode(string output);
    }

    /// <summary>
    /// Default output encoder that performs no transformation.
    /// </summary>
    /// <remarks>
    /// Used by default for full backward compatibility. Commands output exactly as written.
    /// </remarks>
    public class NoOpEncoder : IOutputEncoder
    {
        /// <summary>
        /// Returns the output unchanged.
        /// </summary>
        public string Encode(string output) => output;
    }
}
