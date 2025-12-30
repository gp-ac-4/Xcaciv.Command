using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterDateTime(string name, string raw, DateTime value, bool isValid, string validationError) : AbstractParameterValue<DateTime>(name, raw, value, isValid, validationError)
    {
    }
}
