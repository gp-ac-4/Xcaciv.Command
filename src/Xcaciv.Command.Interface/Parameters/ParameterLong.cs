using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterLong(string name, string raw, long value, bool isValid, string validationError) : AbstractParameterValue<long>(name, raw, value, isValid, validationError)
    {
    }
}
