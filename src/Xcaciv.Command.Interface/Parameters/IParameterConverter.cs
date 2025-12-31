using System.Text.Json;

namespace Xcaciv.Command.Interface.Parameters;

/// <summary>
/// Converts string parameter values to their target types with validation.
/// </summary>
public interface IParameterConverter
{
    /// <summary>
    /// Determines if this converter can convert to the specified type.
    /// </summary>
    /// <param name="targetType">The target type.</param>
    /// <returns>true if this converter supports the type; false otherwise.</returns>
    bool CanConvert(Type targetType);

    /// <summary>
    /// Converts a string value to the target type.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <returns>A result object indicating success/failure and the converted value.</returns>
    ParameterConversionResult Convert(string value, Type targetType);

    /// <summary>
    /// Converts a string value to the target type and extracts validation information.
    /// </summary>
    /// <param name="rawValue">The string value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="error">The error message if conversion fails, null otherwise.</param>
    /// <returns>The converted value if successful, or the raw string if conversion failed.</returns>
    object ConvertWithValidation(string rawValue, Type targetType, out string? error);
}

/// <summary>
/// Result of a parameter conversion attempt.
/// </summary>
public class ParameterConversionResult
{
    /// <summary>
    /// Creates a successful conversion result.
    /// </summary>
    /// <param name="value">The converted value.</param>
    public ParameterConversionResult(object? value)
    {
        Value = value;
        IsSuccess = true;
        ErrorMessage = null;
    }

    /// <summary>
    /// Creates a failed conversion result.
    /// </summary>
    /// <param name="errorMessage">The error message describing why conversion failed.</param>
    public ParameterConversionResult(string errorMessage)
    {
        Value = null;
        IsSuccess = false;
        ErrorMessage = errorMessage ?? "Unknown conversion error";
    }

    /// <summary>
    /// Gets the converted value (null if conversion failed).
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Indicates whether conversion was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message if conversion failed.
    /// </summary>
    public string? ErrorMessage { get; }
}
