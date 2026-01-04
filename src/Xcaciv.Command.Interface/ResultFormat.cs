using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// Indicates the serialization or presentation shape of a command result. 
    /// Intended for consumers to render or post-process output consistently.
    /// </summary>
    public enum ResultFormat
    {
        /// <summary>
        /// Default output matching output type.
        /// </summary>
        [Description("text")]
        General,

        /// <summary>
        /// Structured object output suitable for downstream processing.
        /// </summary>
        [Description("application/text")]
        Object,

        /// <summary>
        /// Comma-separated values intended for tabular data interchange.
        /// </summary>
        [Description("text/csv")]
        CSV,

        /// <summary>
        /// Tab-delimited lines, useful for spreadsheet ingestion.
        /// </summary>
        [Description("application/tdl")]
        TDL,

        /// <summary>
        /// YAML formatted output for configuration-centric workflows.
        /// </summary>
        [Description("application/x-yaml")]
        YAML,

        /// <summary>
        /// JSON formatted output for programmatic consumption.
        /// </summary>
        [Description("application/json")]
        JSON
    }
}
