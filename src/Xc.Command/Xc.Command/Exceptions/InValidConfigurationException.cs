using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xc.Command.Exceptions
{
    public class InValidConfigurationException : XcCommandException
    {
        public InValidConfigurationException(string message) : base(message)
        {
        }

        public InValidConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
