namespace Xcaciv.Command.Interface.Parameters;

/// <summary>
/// Represents a strongly-typed parameter value with validation and conversion support.
/// </summary>
public interface IParameterValue<out T> : IParameterValue
{
    /// <summary>
    /// Gets the converted/validated value in the parameter's target type.
    /// </summary>
    T GetValue();
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
    /// Gets the declared parameter data type (non-null).
    /// </summary>
    Type DataType { get; }

    /// <summary>
    /// Gets the boxed value as produced by conversion (may be InvalidParameterValue when invalid).
    /// </summary>
    object? UntypedValue { get; }

    /// <summary>
    /// Gets any validation error if the value could not be converted or validated.
    /// </summary>
    string? ValidationError { get; }

    /// <summary>
    /// Indicates whether the value is valid and can be safely used.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Retrieves the value as the requested type with validation and type checks.
    /// </summary>
    T GetValue<T>();

    /// <summary>
    /// Attempts to retrieve the value as the requested type.
    /// </summary>
    bool TryGetValue<T>(out T value);
}
