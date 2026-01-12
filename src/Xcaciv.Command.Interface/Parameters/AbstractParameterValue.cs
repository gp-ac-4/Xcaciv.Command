using System;
using System.ComponentModel;

namespace Xcaciv.Command.Interface.Parameters
{
    public abstract class AbstractParameterValue<T> : IParameterValue<T>, IParameterValue
    {
        public object? UntypedValue { get; }

        public string Name { get; }

        public string RawValue { get; } = string.Empty;

        public string? ValidationError { get; }

        public bool IsValid { get; }

        public Type DataType { get; }

        protected AbstractParameterValue(string name, string raw, object? value, bool isValid, string? validationError)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentNullException(nameof(name))
                : name;

            RawValue = raw;
            UntypedValue = value;
            IsValid = isValid;
            ValidationError = validationError;
            DataType = typeof(T);
        }

        public T GetValue()
        {
            return GetValue<T>();
        }

        public TResult GetValue<TResult>()
        {
            // fail quickly if invalid - suggest to check first
            if (!IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot access parameter '{Name}' due to validation error: {ValidationError}\n" +
                    $"Raw value: '{RawValue}'\n" +
                    $"Expected type: {DataType.Name}\n" +
                    "Hint: only access value if valid.");
            }

            // Check requested type against stored datatype
            if (DataType != typeof(TResult))
            {
                throw new InvalidCastException(
                    $"Type mismatch for parameter '{Name}':\n" +
                    $"  Stored as: {DataType.Name}\n" +
                    $"  Requested as: {typeof(TResult).Name}\n" +
                    $"  Raw value: '{RawValue}'\n" +
                    "Hint: Ensure you're requesting the correct type.");
            }

            if (UntypedValue == null && TryConvert(typeof(TResult), out object? converted))
            {
                return (TResult)converted!;
            }

            if (UntypedValue is TResult requested)
            {
                return requested;
            }

            var actualType = UntypedValue.GetType();
            throw new InvalidCastException(
                $"Type mismatch for parameter '{Name}':\n" +
                $"  Stored as: {actualType.Name} (value: {UntypedValue})\n" +
                $"  Requested as: {typeof(TResult).Name}\n" +
                $"  DataType indicates: {DataType.Name}\n" +
                $"  Raw value: '{RawValue}'\n" +
                "Hint: Ensure parameter conversion succeeded and you're requesting the correct type.");
        }

        public bool TryGetValue<TResult>(out TResult value)
        {
            value = default!;

            if (!IsValid || UntypedValue is InvalidParameterValue)
            {
                return false;
            }
            
            if (UntypedValue == null && TryConvert(typeof(TResult), out object? converted))
            {
                value = (TResult)converted!;
                return true;
            }
            
            if (UntypedValue is TResult requested)
            {
                value = requested;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to convert the current raw value to the specified target type.
        /// </summary>
        /// <remarks>This method supports conversion for types that provide a static TryParse method, are
        /// supported by a TypeConverter, or are compatible with Convert.ChangeType. If the conversion fails, the result
        /// parameter is set to null.</remarks>
        /// <param name="targetType">The type to which to attempt to convert the raw value.</param>
        /// <param name="result">When this method returns, contains the converted value if the conversion succeeded; otherwise, null. This
        /// parameter is passed uninitialized.</param>
        /// <returns>true if the conversion was successful; otherwise, false.</returns>
        private bool TryConvert(Type targetType, out object? result)
        {
            result = null;

            // 1. Try built-in TryParse via reflection
            var tryParse = targetType.GetMethod(
                "TryParse",
                new[] { typeof(string), targetType.MakeByRefType() }
            );

            if (tryParse != null)
            {
                var parameters = new object?[] { RawValue, null };
                bool success = (bool)tryParse.Invoke(null, parameters)!;
                if (success)
                {
                    result = parameters[1];
                    return true;
                }
            }

            // 2. Try TypeConverter
            var converter = TypeDescriptor.GetConverter(targetType);
            if (converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    result = converter.ConvertFrom(RawValue);
                    return true;
                }
                catch { }
            }

            // 3. Try Convert.ChangeType
            try
            {
                result = Convert.ChangeType(RawValue, targetType);
                return true;
            }
            catch { }

            return false;
        }

    }
}
