using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface.Attributes
{
    /// <summary>
    /// An unnamed parameter that is determined by the order of the value in the parameters
    /// ordered parameters are required to be passed before named parameters
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CommandParameterOrderedAttribute : AbstractCommandParameter
    {
        private string _defaultValue = "";

        /// <summary>
        /// An unnamed parameter that is determined by the order of the value in the parameters
        /// ordered parameters are required to be passed before named parameters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public CommandParameterOrderedAttribute(string name, string description) 
        { 
            this.Name = name;
            this.ValueDescription = description;
        }
        /// <summary>
        /// input values that are allowed, anything else will throw an error
        /// case is ignored
        /// </summary>
        public string[] AllowedValues 
        { 
            get;
            set 
            {
                field = value ?? [];
                
                // Auto-set default value to first allowed value if not already set
                if (field.Length > 0 && string.IsNullOrEmpty(_defaultValue))
                {
                    _defaultValue = field[0];
                }
                
                // Validate existing default value against new allowed values
                if (!string.IsNullOrEmpty(_defaultValue))
                {
                    ValidateDefaultValue(_defaultValue, field);
                }
            }
        } = [];
        /// <summary>
        /// used when no value is provided
        /// this satisfies the IsRequired flag
        /// </summary>
        public string DefaultValue 
        { 
            get => _defaultValue;
            set 
            {
                ValidateDefaultValue(value, AllowedValues);
                _defaultValue = value;
            }
        }
        /// <summary>
        /// indicates if the parameter is required
        /// </summary>
        public bool IsRequired { get; set; } = true;
        /// <summary>
        /// indicates this value is what is populated when a pipe is used
        /// only the first parameter specified for pipeline population will be used
        /// </summary>
        public bool UsePipe { get; set; } = false;

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
    }
}
