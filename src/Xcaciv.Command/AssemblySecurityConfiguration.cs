using System;
using Xcaciv.Loader;

namespace Xcaciv.Command;

/// <summary>
/// Configuration for assembly loading security policies.
/// Controls how plugins are validated and loaded.
/// </summary>
public class AssemblySecurityConfiguration
{
    /// <summary>
    /// Security policy to apply when loading plugin assemblies.
    /// Default: AssemblySecurityPolicy.Strict (requires explicit allowlist)
    /// </summary>
    public AssemblySecurityPolicy SecurityPolicy { get; set; } = AssemblySecurityPolicy.Strict;

    /// <summary>
    /// Whether to allow reflection-based emission in loaded assemblies.
    /// Default: false (disabled for security)
    /// </summary>
    public bool AllowReflectionEmit { get; set; } = false;

    /// <summary>
    /// Whether to enforce strict directory restrictions.
    /// When true, plugins cannot load assemblies outside their own directory.
    /// Default: true (strict enforcement)
    /// </summary>
    public bool EnforceBasePathRestriction { get; set; } = true;

    /// <summary>
    /// Allowed assembly name prefixes for plugin dependencies.
    /// If empty, allows all assemblies from the restricted directory.
    /// Example: ["MyApp.", "Common."] restricts to those namespaces.
    /// </summary>
    public string[] AllowedDependencyPrefixes { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Validate configuration for consistency.
    /// </summary>
    public void Validate()
    {
        if (SecurityPolicy == AssemblySecurityPolicy.Default && AllowedDependencyPrefixes.Length > 0)
        {
            throw new InvalidOperationException(
                "Cannot specify AllowedDependencyPrefixes with AssemblySecurityPolicy.Default. " +
                "Use AssemblySecurityPolicy.Strict with an allowlist instead.");
        }
    }
}
