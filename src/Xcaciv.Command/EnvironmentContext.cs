using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command
{
    public class EnvironmentContext : IEnvironmentContext
    {
        /// <summary>
        /// Thread safe collection of env vars
        /// MUST be set when creating a child!
        /// </summary>
        protected ConcurrentDictionary<string, string> EnvironmentVariables { get; set; } = new ConcurrentDictionary<string, string>();

        public bool HasChanged {  get; private set; }

        public Guid Id {  get; } = Guid.NewGuid();

        public string Name { get; set; } = "Environment";

        public Guid? Parent {  get; protected set; }

        public EnvironmentContext() { }

        public EnvironmentContext(Dictionary<string, string> environment)
        {
            this.UpdateEnvironment(environment);
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task<IEnvironmentContext> GetChild(string[]? childArguments = null)
        {
            var childValues = this.GetEnvinronment();
            var child = new EnvironmentContext(childValues);
            return Task.FromResult<IEnvironmentContext>(child);
        }
        

        /// <summary>
        /// <see cref="Xcaciv.Command.Interface.IEnvironmentContext"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public virtual void SetValue(string key, string addValue)
        {
            // make case insensitive var names
            key = key.ToUpper();
            EnvironmentVariables.AddOrUpdate(key, addValue, (key, value) =>
            {
                Trace.WriteLine($"Environment value {key} changed from {value} to {addValue}.");
                return addValue;
            });

            this.HasChanged = true;
        }
        /// <summary>
        /// <see cref="Xcaciv.Command.Interface.IEnvironmentContext"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string GetValue(string key)
        {
            // make case insensitive var names
            key = key.ToUpper();

            string? returnValue;
            EnvironmentVariables.TryGetValue(key, out returnValue);
            return returnValue ?? String.Empty;
        }

        public Dictionary<string, string> GetEnvinronment()
        {
            return EnvironmentVariables.ToDictionary();
        }

        public void UpdateEnvironment(Dictionary<string, string> dictionary)
        {
            foreach (var pair in dictionary)
            {
                SetValue(pair.Key, pair.Value);
            }
        }
    }
}
