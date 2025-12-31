namespace Xcaciv.Command.Interface.Parameters;

/// <summary>
/// Represents a strongly-typed parameter value with validation and conversion support.
/// </summary>
public interface IParameterValue<out T>
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the raw string value as provided by the user.
    /// </summary>
    string RawValue { get; }

    /// <summary>
    /// Gets the converted/validated value in the parameter's target type.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Gets any validation error if the value could not be converted or validated.
    /// </summary>
    string? ValidationError { get; }

    /// <summary>
    /// Indicates whether the value is valid and can be safely used.
    /// </summary>
    bool IsValid { get; }
}

/// <summary>
/// Non-generic base interface for parameter values to allow collection storage.
/// </summary>
public interface IParameterValue
{
    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the raw string value as provided by the user.
    /// </summary>
    string RawValue { get; }

    /// <summary>
    /// Gets any validation error if the value could not be converted or validated.
    /// </summary>
    string? ValidationError { get; }

    /// <summary>
    /// Indicates whether the value is valid and can be safely used.
    /// </summary>
    bool IsValid { get; }
}
