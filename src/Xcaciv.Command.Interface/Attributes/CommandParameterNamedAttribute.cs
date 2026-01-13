using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterNamedAttribute : AbstractCommandParameter
    {
        private string _defaultValue = "";
        private string[] _allowedValues = [];

        public CommandParameterNamedAttribute(string name, string description) 
        { 
            this.Name = name;
            this.ValueDescription = description;
        }
        /// <summary>
        /// used when no value is provided
        /// this satisfies the IsRequired flag
        /// </summary>
        public string DefaultValue 
        { 
            get => _defaultValue;
            set 
            {
                ValidateDefaultValue(value, _allowedValues);
                _defaultValue = value;
            }
        }

        public string ShortAlias { get; set; } = "";
        /// <summary>
        /// throws an error if this value is not provided
        /// </summary>
        public bool IsRequired { get; set; } = false;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// when multiple are specified
        /// </summary>
        public bool UsePipe { get; set; } = false;

        /// <summary>
        /// input values that are allowed, anything else will throw an error
        /// case is ignored
        /// </summary>
        public string[] AllowedValues 
        { 
            get => _allowedValues;
            set 
            {
                _allowedValues = value ?? [];
                
                // Auto-set default value to first allowed value if not already set
                if (_allowedValues.Length > 0 && string.IsNullOrEmpty(_defaultValue))
                {
                    _defaultValue = _allowedValues[0];
                }
                
                // Validate existing default value against new allowed values
                if (!string.IsNullOrEmpty(_defaultValue))
                {
                    ValidateDefaultValue(_defaultValue, _allowedValues);
                }
            }
        }

        private void ValidateDefaultValue(string defaultValue, string[] allowedValues)
        {
            if (string.IsNullOrEmpty(defaultValue) || allowedValues == null || allowedValues.Length == 0)
            {
                return;
            }

            if (!allowedValues.Contains(defaultValue, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Default value '{defaultValue}' is not in the allowed values list for parameter '{Name}'. " +
                    $"Allowed values: {string.Join(", ", allowedValues)}");
            }
        }

        public override string GetIndicator()
        {
            return $"-{_helpName}";
        }

        public override string GetValueDescription()
        {
            string description = ValueDescription;
            if (AllowedValues.Length > 0)
            {
                description += $" (Allowed values: {string.Join(", ", AllowedValues)})";
            }
            return description;
        }
    }
}
