using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xcaciv.Command.Interface.Parameters;

namespace Xcaciv.Command.Core.Parameters
{
    /// <summary>
    /// Collection of strongly-typed parameter values with convenient access methods.
    /// </summary>
    public class ParameterCollection : IEnumerable<IParameterValue>
    {
        private readonly Dictionary<string, IParameterValue> _parameters;

        public ParameterCollection()
        {
            _parameters = new Dictionary<string, IParameterValue>(StringComparer.OrdinalIgnoreCase);
        }

        public ParameterCollection(IEnumerable<IParameterValue> parameters)
            : this()
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            foreach (var param in parameters)
            {
                Add(param);
            }
        }

        /// <summary>
        /// Gets the number of parameters in the collection.
        /// </summary>
        public int Count => _parameters.Count;

        /// <summary>
        /// Adds a parameter to the collection.
        /// </summary>
        public void Add(IParameterValue parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            _parameters[parameter.Name] = parameter;
        }

        /// <summary>
        /// Adds a parameter to the collection using collection initializer syntax.
        /// </summary>
        public void Add(string name, IParameterValue parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            _parameters[name] = parameter;
        }

        /// <summary>
        /// Checks if a parameter with the specified name exists.
        /// </summary>
        public bool Contains(string name)
        {
            return _parameters.ContainsKey(name);
        }

        /// <summary>
        /// Gets a parameter by name, or null if not found.
        /// </summary>
        public ParameterValue? GetParameter(string name)
        {
            if (_parameters.TryGetValue(name, out var parameter))
                return parameter as ParameterValue;

            return null;
        }

        /// <summary>
        /// Gets a parameter by name, throwing if not found.
        /// </summary>
        public ParameterValue GetParameterRequired(string name)
        {
            if (!_parameters.TryGetValue(name, out var parameter))
                throw new KeyNotFoundException($"Parameter '{name}' not found.");

            return (ParameterValue)parameter;
        }

        /// <summary>
        /// Gets a parameter by name.
        /// </summary>
        public IParameterValue Get(string name)
        {
            if (!_parameters.TryGetValue(name, out var parameter))
                throw new KeyNotFoundException($"Parameter '{name}' not found.");

            return parameter;
        }

        /// <summary>
        /// Tries to get a parameter by name.
        /// </summary>
        public bool TryGet(string name, out IParameterValue? parameter)
        {
            return _parameters.TryGetValue(name, out parameter);
        }

        /// <summary>
        /// Gets a strongly-typed parameter value.
        /// </summary>
        public T GetValue<T>(string name)
        {
            var parameter = Get(name);
            
            if (parameter is IParameterValue<T> typedParam)
            {
                if (!typedParam.IsValid)
                    throw new InvalidOperationException($"Parameter '{name}' has validation error: {typedParam.ValidationError}");

                return typedParam.Value;
            }

            throw new InvalidCastException($"Parameter '{name}' is not of type {typeof(T).Name}");
        }

        /// <summary>
        /// Gets a value type parameter value.
        /// </summary>
        public T GetAsValueType<T>(string name) where T : struct
        {
            var parameter = GetParameterRequired(name);
            return parameter.AsValueType<T>();
        }

        /// <summary>
        /// Gets a strongly-typed parameter value or returns a default value if not found or invalid.
        /// </summary>
        public T GetValueOrDefault<T>(string name, T defaultValue = default!)
        {
            if (!TryGet(name, out var parameter))
                return defaultValue;

            if (parameter is IParameterValue<T> typedParam && typedParam.IsValid)
                return typedParam.Value;

            return defaultValue;
        }

        /// <summary>
        /// Gets all parameter names in the collection.
        /// </summary>
        public IEnumerable<string> GetNames()
        {
            return _parameters.Keys;
        }

        /// <summary>
        /// Gets all invalid parameters in the collection.
        /// </summary>
        public IEnumerable<IParameterValue> GetInvalidParameters()
        {
            return _parameters.Values.Where(p => !p.IsValid);
        }

        /// <summary>
        /// Returns true if all parameters in the collection are valid.
        /// </summary>
        public bool IsValid => !_parameters.Values.Any(p => !p.IsValid);

        /// <summary>
        /// Returns true if all parameters in the collection are valid.
        /// </summary>
        public bool AreAllValid()
        {
            return IsValid;
        }

        public IEnumerator<IParameterValue> GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Indexer to access and set parameters by name.
        /// </summary>
        public IParameterValue this[string name]
        {
            get => Get(name);
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _parameters[name] = value;
            }
        }
    }
}
