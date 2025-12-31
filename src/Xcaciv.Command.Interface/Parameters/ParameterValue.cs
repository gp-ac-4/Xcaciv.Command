using System;

namespace Xcaciv.Command.Interface.Parameters
{
    /// <summary>
    /// Generic string parameter value implementation.
    /// </summary>
    public class ParameterValue : AbstractParameterValue<object>
    {
        public Type DataType { get; }

        /// <summary>
        /// Creates a simple string parameter value.
        /// </summary>
        public ParameterValue(string name, string raw, string value, bool isValid, string validationError)
            : base(name, raw, value, isValid, validationError)
        {
            DataType = typeof(string);
        }

        /// <summary>
        /// Creates a parameter value with a converted value and type information.
        /// This constructor is typically called by factory methods that handle conversion.
        /// </summary>
        public ParameterValue(string name, string raw, object value, Type dataType, bool isValid, string validationError)
            : base(name, raw, value, isValid, validationError)
        {
            DataType = dataType ?? throw new ArgumentNullException(nameof(dataType));
        }

        /// <summary>
        /// Gets the value as the specified type.
        /// </summary>
        /// <typeparam name="T">The target type to retrieve.</typeparam>
        /// <returns>The parameter value cast to type T.</returns>
        /// <exception cref="InvalidOperationException">Thrown when parameter has validation errors.</exception>
        /// <exception cref="InvalidCastException">Thrown when type conversion is impossible.</exception>
        public T As<T>()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot access parameter '{Name}' due to validation error: {ValidationError}\n" +
                    $"Raw value: '{RawValue}'\n" +
                    $"Expected type: {DataType.Name}");
            }

            // Handle null for reference types
            if (Value == null)
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot convert null to non-nullable value type {typeof(T).Name} " +
                        $"for parameter '{Name}'.");
                }
                return default!;
            }

            // Attempt direct cast
            if (Value is T typedValue)
            {
                return typedValue;
            }

            // Provide detailed diagnostic for cast failure
            var actualType = Value.GetType();
            var requestedType = typeof(T);
            
            throw new InvalidCastException(
                $"Type mismatch for parameter '{Name}':\n" +
                $"  Stored as: {actualType.Name} (value: {Value})\n" +
                $"  Requested as: {requestedType.Name}\n" +
                $"  DataType indicates: {DataType.Name}\n" +
                $"  Raw value: '{RawValue}'\n" +
                $"Hint: Ensure parameter conversion succeeded and you're requesting the correct type.");
        }
    }
}
