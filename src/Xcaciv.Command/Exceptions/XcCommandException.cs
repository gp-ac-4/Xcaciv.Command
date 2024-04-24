using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Exceptions
{
    public abstract class XcCommandException : Exception
    {
        public XcCommandException(string message) : base(message)
        {
        }

        public XcCommandException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
