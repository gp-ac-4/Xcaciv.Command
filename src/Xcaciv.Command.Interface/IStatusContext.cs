using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public interface IStatusContext
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
    }
}
