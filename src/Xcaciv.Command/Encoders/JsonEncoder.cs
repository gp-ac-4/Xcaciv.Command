using System;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Encoders
{
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
