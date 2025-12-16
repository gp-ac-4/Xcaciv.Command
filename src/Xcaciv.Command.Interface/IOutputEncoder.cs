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

    /// <summary>
    /// HTML encoder that escapes special characters for safe web display.
    /// </summary>
    /// <remarks>
    /// Escapes: &lt;, &gt;, &amp;, &quot;, and other HTML special characters.
    /// Use this when command output is displayed in a web UI or HTML context.
    /// </remarks>
    public class HtmlEncoder : IOutputEncoder
    {
        /// <summary>
        /// HTML-encodes the output for safe display in web browsers.
        /// </summary>
        public string Encode(string output)
        {
            return System.Net.WebUtility.HtmlEncode(output);
        }
    }

    /// <summary>
    /// JSON encoder that escapes special characters for JSON strings.
    /// </summary>
    /// <remarks>
    /// Escapes: quotes, backslashes, control characters, and other JSON special characters.
    /// Use this when command output is serialized as JSON (e.g., API responses).
    /// </remarks>
    public class JsonEncoder : IOutputEncoder
    {
        /// <summary>
        /// JSON-encodes the output for safe inclusion in JSON strings.
        /// </summary>
        public string Encode(string output)
        {
            // Use System.Text.Json for JSON string escaping
            var json = System.Text.Json.JsonSerializer.Serialize(output);
            // Remove surrounding quotes that Serialize adds
            return json.Length > 2 ? json.Substring(1, json.Length - 2) : json;
        }
    }
}
