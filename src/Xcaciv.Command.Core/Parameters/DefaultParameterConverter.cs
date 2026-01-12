using System.Globalization;
using System.Text.Json;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Core.Parameters;

/// <summary>
/// Default parameter converter supporting scalar types: string, int, float, decimal, bool, Guid, and JSON.
/// </summary>
public class DefaultParameterConverter : IParameterConverter
{
    private static readonly HashSet<Type> SupportedTypes = new()
    {
        typeof(string),
        typeof(int),
        typeof(long),
        typeof(double),
        typeof(float),
        typeof(decimal),
        typeof(bool),
        typeof(Guid),
        typeof(DateTime),
        typeof(JsonElement)
    };

    public bool CanConvert(Type targetType)
    {
        if (targetType == null)
            return false;

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return SupportedTypes.Contains(underlyingType);
    }

    public ParameterConversionResult Convert(string value, Type targetType)
    {
        if (targetType == null)
            return new ParameterConversionResult("Target type cannot be null.");

        if (string.IsNullOrEmpty(value) && targetType != typeof(string))
        {
            return new ParameterConversionResult("Cannot convert empty string to non-string type.");
        }

        try
        {
            // Handle string type
            if (targetType == typeof(string))
            {
                return new ParameterConversionResult((object?)value);
            }

            // Handle nullable types
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle int
            if (underlyingType == typeof(int))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid integer.");
            }

            // Handle long
            if (underlyingType == typeof(long))
            {
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid long integer.");
            }

            // Handle float
            if (underlyingType == typeof(float))
            {
                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid float.");
            }

            // Handle double
            if (underlyingType == typeof(double))
            {
                if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid double.");
            }

            // Handle decimal
            if (underlyingType == typeof(decimal))
            {
                if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid decimal.");
            }

            // Handle bool
            if (underlyingType == typeof(bool))
            {
                if (bool.TryParse(value, out var result))
                    return new ParameterConversionResult(result);
                
                // Accept common boolean representations
                var lowerValue = value.ToLowerInvariant();
                if (lowerValue == "1" || lowerValue == "yes" || lowerValue == "on" || lowerValue == "true")
                    return new ParameterConversionResult(true);
                if (lowerValue == "0" || lowerValue == "no" || lowerValue == "off" || lowerValue == "false")
                    return new ParameterConversionResult(false);
                
                return new ParameterConversionResult($"'{value}' is not a valid boolean.");
            }

            // Handle Guid
            if (underlyingType == typeof(Guid))
            {
                if (Guid.TryParse(value, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid GUID.");
            }

            // Handle DateTime
            if (underlyingType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var result))
                    return new ParameterConversionResult(result);
                return new ParameterConversionResult($"'{value}' is not a valid DateTime.");
            }

            // Handle JsonElement
            if (targetType == typeof(JsonElement))
            {
                try
                {
                    using var doc = JsonDocument.Parse(value);
                    return new ParameterConversionResult(doc.RootElement.Clone());
                }
                catch (JsonException ex)
                {
                    return new ParameterConversionResult($"'{value}' is not valid JSON: {ex.Message}");
                }
            }

            return new ParameterConversionResult($"Unsupported type '{targetType.Name}'.");
        }
        catch (Exception ex)
        {
            return new ParameterConversionResult($"Conversion failed: {ex.Message}");
        }
    }

    public object ConvertWithValidation(string rawValue, Type targetType, out string? error)
    {
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));

        error = null;
        
        // String passthrough
        if (targetType == typeof(string))
        {
            return rawValue;
        }

        // Attempt conversion
        var result = Convert(rawValue, targetType);
        if (!result.IsSuccess)
        {
            error = result.ErrorMessage;
            // Return sentinel instead of raw string
            return InvalidParameterValue.Instance;
        }

        // Return proper default for target type instead of empty string
        if (result.Value == null)
        {
            return GetDefaultValue(targetType);
        }

        return result.Value;
    }

    public object ValidateAndConvert(string parameterName, string rawValue, Type targetType, out string validationError, out bool isValid)
    {
        if (string.IsNullOrEmpty(parameterName))
            throw new ArgumentException("Parameter name cannot be null or empty.", nameof(parameterName));
        
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));
        
        if (!CanConvert(targetType))
            throw new ArgumentException(
                $"Converter does not support type '{targetType.Name}' for parameter '{parameterName}'.", 
                nameof(targetType));

        var convertedValue = ConvertWithValidation(rawValue, targetType, out var error);
        isValid = error == null;
        validationError = error ?? string.Empty;
        
        // Validate type consistency for successful conversions
        if (isValid && convertedValue != null && convertedValue is not InvalidParameterValue)
        {
            var actualType = convertedValue.GetType();
            
            // Allow exact match or assignable types (handles boxing)
            if (actualType != targetType && !targetType.IsAssignableFrom(actualType))
            {
                // Check if it's a boxed value type matching the target
                var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
                
                if (actualType != underlyingType)
                {
                    throw new InvalidOperationException(
                        $"Type safety violation: Converter returned {actualType.Name} " +
                        $"but parameter '{parameterName}' expects {targetType.Name}.");
                }
            }
        }
        
        // Return sentinel if conversion failed or value is null, otherwise return the converted value
        return convertedValue ?? InvalidParameterValue.Instance;
    }

    public T ValidateAndConvert<T>(string parameterName, string rawValue, out string validationError, out bool isValid)
    {
        var convertedValue = ValidateAndConvert(parameterName, rawValue, typeof(T), out validationError, out isValid);

        if (!isValid)
        {
            return default!;
        }

        if (convertedValue is T typedValue)
        {
            return typedValue;
        }

        var actualType = convertedValue?.GetType().Name ?? "null";
        throw new InvalidOperationException(
            $"Type safety violation: Converter returned {actualType} but parameter '{parameterName}' expects {typeof(T).Name}.");
    }

    /// <summary>
    /// Gets the default value for a type (default(T) equivalent).
    /// </summary>
    private static object GetDefaultValue(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type)!;
        }
        
        return InvalidParameterValue.Instance;  // For reference types
    }
}
