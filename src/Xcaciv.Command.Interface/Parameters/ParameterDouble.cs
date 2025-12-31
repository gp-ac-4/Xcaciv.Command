using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterDouble(string name, string raw, double value, bool isValid, string validationError) : AbstractParameterValue<double>(name, raw, value, isValid, validationError)
    {
    }
}
