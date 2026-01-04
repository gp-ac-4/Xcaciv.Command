using System;
using System.Collections.Generic;
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
        General,

        /// <summary>
        /// Structured object output suitable for downstream processing.
        /// </summary>
        Object,

        /// <summary>
        /// Comma-separated values intended for tabular data interchange.
        /// </summary>
        CSV,

        /// <summary>
        /// Tab-delimited lines, useful for spreadsheet ingestion.
        /// </summary>
        TDL,

        /// <summary>
        /// YAML formatted output for configuration-centric workflows.
        /// </summary>
        YAML,

        /// <summary>
        /// JSON formatted output for programmatic consumption.
        /// </summary>
        JSON
    }
}
