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
        error = null;
        
        if (targetType == typeof(string))
        {
            return rawValue;
        }

        var result = Convert(rawValue, targetType);
        if (!result.IsSuccess)
        {
            error = result.ErrorMessage;
            return rawValue;
        }

        return result.Value ?? string.Empty;
    }
}
