using System;
using System.Collections.Generic;
using System.Linq;

namespace Xcaciv.Command.Interface;

/// <summary>
/// Configuration for parameter masking in audit logs.
/// Allows redaction of sensitive parameters based on parameter names or patterns.
/// </summary>
public sealed class AuditMaskingConfiguration
{
    /// <summary>
    /// Parameter names that should be completely redacted in audit logs.
    /// Case-insensitive comparison.
    /// </summary>
    public ISet<string> RedactedParameterNames { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "password",
        "pwd",
        "secret",
        "token",
        "apikey",
        "api_key",
        "connectionstring",
        "conn",
        "credential"
    };

    /// <summary>
    /// Patterns for parameter names that should be redacted.
    /// </summary>
    public ISet<string> RedactedParameterPatterns { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "*password*",
        "*secret*",
        "*token*",
        "*key*",
        "*credential*"
    };

    /// <summary>
    /// The replacement text for redacted parameters.
    /// </summary>
    public string RedactionPlaceholder { get; init; } = "[REDACTED]";

    /// <summary>
    /// Determines if a parameter name should be redacted based on configured rules.
    /// </summary>
    /// <param name="parameterName">The parameter name to check.</param>
    /// <returns>True if the parameter should be redacted; otherwise false.</returns>
    public bool ShouldRedact(string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            return false;

        // Exact match
        if (RedactedParameterNames.Contains(parameterName))
            return true;

        // Pattern match
        foreach (var pattern in RedactedParameterPatterns)
        {
            if (MatchesPattern(parameterName, pattern))
                return true;
        }

        return false;
    }

    private bool MatchesPattern(string value, string pattern)
    {
        // Simple wildcard matching (* = any characters)
        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        {
            var substring = pattern.Trim('*');
            return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.StartsWith("*"))
        {
            var suffix = pattern.TrimStart('*');
            return value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.EndsWith("*"))
        {
            var prefix = pattern.TrimEnd('*');
            return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            return value.Equals(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Apply masking to an array of parameters based on parameter position and naming convention.
    /// </summary>
    /// <param name="parameters">The parameters to potentially mask.</param>
    /// <returns>A new array with sensitive parameters redacted.</returns>
    public string[] ApplyMasking(string[] parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return parameters ?? Array.Empty<string>();

        var masked = new string[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameterValue = parameters[i];
            // Check if parameter looks like --name=value or -name value
            if (parameterValue.StartsWith("--") || parameterValue.StartsWith("-"))
            {
                var parts = parameterValue.Split('=', 2);
                var parameterName = parts[0].TrimStart('-');
                masked[i] = ShouldRedact(parameterName) && parts.Length == 2
                    ? $"{parts[0]}={RedactionPlaceholder}"
                    : parameterValue;
            }
            else
            {
                masked[i] = parameterValue;
            }
        }
        return masked;
    }
}
