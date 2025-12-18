using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    /// <summary>
    /// Manages environment variables and context isolation for command execution.
    /// Commands execute in isolated child contexts; parent environment is updated
    /// only if the command has ModifiesEnvironment=true.
    /// </summary>
    /// <remarks>
    /// The environment context provides:
    /// - Case-insensitive environment variable storage and retrieval
    /// - Child context creation for command isolation
    /// - Change tracking (HasChanged property)
    /// - Audit logging integration
    /// 
    /// Security: Child contexts are isolated from parent contexts.
    /// A command cannot modify the parent environment unless explicitly allowed
    /// via the ModifiesEnvironment flag on its CommandDescription.
    /// </remarks>
    public interface IEnvironmentContext: ICommandContext<IEnvironmentContext>
    {
        /// <summary>
        /// Sets an environment variable to the specified value.
        /// </summary>
        /// <param name="key">The variable name (case-insensitive; stored as uppercase).</param>
        /// <param name="value">The variable value.</param>
        /// <remarks>
        /// If the variable already exists, its value is overwritten.
        /// This operation marks the context as HasChanged = true.
        /// If an audit logger is configured, the change is logged.
        /// </remarks>
        void SetValue(string key, string value);

        /// <summary>
        /// Retrieves an environment variable value.
        /// </summary>
        /// <param name="key">The variable name (case-insensitive).</param>
        /// <param name="defaultValue">Value to return if variable not found (default: empty string).</param>
        /// <param name="storeDefault">If true and variable not found, stores the default value (default: true).</param>
        /// <returns>The variable value, or defaultValue if not found.</returns>
        /// <remarks>
        /// Variable lookup is case-insensitive. All keys are normalized to uppercase internally.
        /// If storeDefault=true and variable not found, the default value is automatically stored.
        /// </remarks>
        string GetValue(string key, string defaultValue = "", bool storeDefault = true);

        /// <summary>
        /// Retrieves all environment variables as a dictionary.
        /// </summary>
        /// <returns>A new dictionary containing all current environment variables.</returns>
        /// <remarks>
        /// Returns a snapshot of the current environment. Modifications to the returned
        /// dictionary do not affect the environment context; use SetValue() to modify variables.
        /// </remarks>
        Dictionary<string, string> GetEnvironment();

        /// <summary>
        /// Indicates whether this context has modified any environment variables.
        /// </summary>
        /// <value>true if any variables have been added or changed; false if unchanged.</value>
        /// <remarks>
        /// Used by the framework to determine whether to update parent environment
        /// after command execution (if ModifiesEnvironment=true).
        /// </remarks>
        bool HasChanged { get; }

        /// <summary>
        /// Synchronizes environment variables from another environment or dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary of variables to merge into this context.</param>
        /// <remarks>
        /// Used by the framework to update the parent environment after a command
        /// execution (if the command has ModifiesEnvironment=true).
        /// Overwrites any existing variables with matching keys.
        /// </remarks>
        void UpdateEnvironment(Dictionary<string, string> dictionary);
    }
}
