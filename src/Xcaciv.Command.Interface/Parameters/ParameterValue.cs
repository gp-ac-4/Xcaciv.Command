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
            : base(name, raw, converter.ConvertWithValidation(raw, targetType, out var error), error == null, error ?? string.Empty)
        {
            DataType = targetType;
        }

        /// <summary>
        /// Gets the value as the specified type.
        /// </summary>
        public T As<T>()
        {
            if (!IsValid)
                throw new InvalidOperationException($"Parameter '{Name}' has validation error: {ValidationError}");

            if (Value is T typedValue)
                return typedValue;

            throw new InvalidCastException($"Cannot cast parameter '{Name}' to type {typeof(T).Name}");
        }
    }
}
