using System;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Encoders
{
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
}
