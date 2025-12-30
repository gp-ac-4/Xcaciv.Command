using System;

namespace Xcaciv.Command.Interface.Parameters
{
    /// <summary>
    /// Generic string parameter value implementation.
    /// </summary>
    public class ParameterValue : AbstractParameterValue<object>
    {
        public Type DataType { get; }

        public ParameterValue(string name, string raw, string value, bool isValid, string validationError)
            : base(name, raw, value, isValid, validationError)
        {
            DataType = typeof(string);
        }

        /// <summary>
        /// Creates a parameter value with automatic type conversion.
        /// </summary>
        public ParameterValue(string name, string raw, Type targetType, IParameterConverter converter)
            : base(name, raw, ConvertValue(raw, targetType, converter, out var error, out var dataType), error == null, error ?? string.Empty)
        {
            DataType = dataType;
        }

        private static object ConvertValue(string raw, Type targetType, IParameterConverter converter, out string? error, out Type actualType)
        {
            error = null;
            actualType = targetType;
            
            if (targetType == typeof(string))
            {
                return raw;
            }

            var result = converter.Convert(raw, targetType);
            if (!result.IsSuccess)
            {
                error = result.ErrorMessage;
                return raw;
            }

            return result.Value ?? string.Empty;
        }

        /// <summary>
        /// Gets the value as the specified reference type.
        /// </summary>
        public T As<T>() where T : class
        {
            if (!IsValid)
                throw new InvalidOperationException($"Parameter '{Name}' has validation error: {ValidationError}");

            if (Value is T typedValue)
                return typedValue;

            throw new InvalidCastException($"Cannot cast parameter '{Name}' to type {typeof(T).Name}");
        }

        /// <summary>
        /// Gets the value as the specified value type.
        /// </summary>
        public T AsValueType<T>() where T : struct
        {
            if (!IsValid)
                throw new InvalidOperationException($"Parameter '{Name}' has validation error: {ValidationError}");

            if (Value is T typedValue)
                return typedValue;

            throw new InvalidCastException($"Cannot cast parameter '{Name}' to type {typeof(T).Name}");
        }
    }
}
