using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public interface IEnvironment
    {
        /// <summary>
        /// replace status text with new text
        /// used for static status output
        /// SHOULD NOT CONTAIN ACTUAL RESULT
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SetStatusMessage(string message);
        /// <summary>
        /// adds to the trace collection
        /// can be output to the screen when being verbose
        /// messages should help troubleshooting for developers and users
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task AddTraceMessage(string message);
        /// <summary>
        /// set proces progress based on total
        /// </summary>
        /// <param name="total"></param>
        /// <param name="step"></param>
        /// <returns>whole number signifying percentage</returns>
        Task<int> SetProgress(int total, int step);
        /// <summary>
        /// signal that the status is complete
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Complete(string? message);
        /// <summary>
        /// add a value to the environment, across commands
        /// the storage mechanism should be apropriate to the running environment
        /// probably a ConcurrentDictionary<TKey,TValue>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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
        bool ValuesChanged { get; }
        /// <summary>
        /// sync values in envronment
        /// </summary>
        /// <param name="dictionary"></param>
        void UpdateEnvironment(Dictionary<string, string> dictionary);
    }
}
