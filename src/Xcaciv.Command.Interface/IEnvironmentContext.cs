using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public interface IEnvironmentContext: ICommandContext<IEnvironmentContext>
    {
        void SetValue(string key, string value);
        /// <summary>
        /// retrieve global environment value
        /// </summary>
        /// <param name="key"></param>
        /// <returns>String.Empty if not found</returns>
        string GetValue(string key);
        /// <summary>
        /// captures the environment values and returns them
        /// </summary>
        /// <returns></returns>
        Dictionary<string, string> GetEnvinronment();
        /// <summary>
        /// indicates that the environment variables has changed
        /// </summary>
        bool HasChanged { get; }
        /// <summary>
        /// sync values in envronment
        /// </summary>
        /// <param name="dictionary"></param>
        void UpdateEnvironment(Dictionary<string, string> dictionary);
    }
}
