using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class AbstractParameterValue<T> : IParameterValue<T>, IParameterValue
    {
        public T Value { get; }
        public string Name { get; }

        public string RawValue { get; } = string.Empty;

        public string? ValidationError { get; }

        public bool IsValid { get; }

        public AbstractParameterValue(string name, string raw, T value, bool isValid, string validationError)
        {
            RawValue = raw ?? string.Empty;
            Value = value;
            Name = name;

            IsValid = isValid;
            ValidationError = validationError;
        }
    }
}
