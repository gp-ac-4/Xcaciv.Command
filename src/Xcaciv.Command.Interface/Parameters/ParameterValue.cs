using System;

namespace Xcaciv.Command.Interface.Parameters
{
    /// <summary>
    /// Strongly-typed parameter value backed by boxed storage to allow invalid sentinels.
    /// </summary>
    public class ParameterValue<T> : AbstractParameterValue<T>
    {
        public ParameterValue(string name, string raw, object? value, bool isValid, string? validationError)
            : base(name, raw, value, isValid, validationError)
        {
        }
    }

    /// <summary>
    /// Factory for creating typed parameter values when the target type is only known at runtime.
    /// </summary>
    public static class ParameterValue
    {
        public static IParameterValue Create(string name, string raw, object? value, Type dataType, bool isValid, string? validationError)
        {
            if (dataType == null)
                throw new ArgumentNullException(nameof(dataType));

            var constructedType = typeof(ParameterValue<>).MakeGenericType(dataType);
            var instance = Activator.CreateInstance(constructedType, name, raw, value, isValid, validationError)
                           ?? throw new InvalidOperationException($"Failed to create ParameterValue for type {dataType.Name}.");

            return (IParameterValue)instance;
        }
    }
}
