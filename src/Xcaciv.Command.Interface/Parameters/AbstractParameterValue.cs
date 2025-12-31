using System;

namespace Xcaciv.Command.Interface.Parameters
{
    public abstract class AbstractParameterValue<T> : IParameterValue<T>, IParameterValue
    {
        private readonly object? _boxedValue;

        public string Name { get; }

        public string RawValue { get; } = string.Empty;

        public string? ValidationError { get; }

        public bool IsValid { get; }

        public Type DataType { get; }

        public object? UntypedValue => _boxedValue;

        protected AbstractParameterValue(string name, string raw, object? value, bool isValid, string? validationError)
        {
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentNullException(nameof(name))
                : name;

            RawValue = raw ?? string.Empty;
            _boxedValue = value;
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
            if (!IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot access parameter '{Name}' due to validation error: {ValidationError}\n" +
                    $"Raw value: '{RawValue}'\n" +
                    $"Expected type: {DataType.Name}");
            }

            if (_boxedValue == null)
            {
                if (typeof(TResult).IsValueType && Nullable.GetUnderlyingType(typeof(TResult)) == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot convert null to non-nullable value type {typeof(TResult).Name} for parameter '{Name}'.");
                }

                return default!;
            }

            if (_boxedValue is InvalidParameterValue)
            {
                throw new InvalidOperationException(
                    $"Parameter '{Name}' is invalid and cannot be accessed. Validation error: {ValidationError}");
            }

            if (_boxedValue is TResult requested)
            {
                return requested;
            }

            var actualType = _boxedValue.GetType();
            throw new InvalidCastException(
                $"Type mismatch for parameter '{Name}':\n" +
                $"  Stored as: {actualType.Name} (value: {_boxedValue})\n" +
                $"  Requested as: {typeof(TResult).Name}\n" +
                $"  DataType indicates: {DataType.Name}\n" +
                $"  Raw value: '{RawValue}'\n" +
                "Hint: Ensure parameter conversion succeeded and you're requesting the correct type.");
        }

        public bool TryGetValue<TResult>(out TResult value)
        {
            value = default!;

            if (!IsValid || _boxedValue == null || _boxedValue is InvalidParameterValue)
            {
                return false;
            }

            if (_boxedValue is TResult requested)
            {
                value = requested;
                return true;
            }

            return false;
        }
    }
}
