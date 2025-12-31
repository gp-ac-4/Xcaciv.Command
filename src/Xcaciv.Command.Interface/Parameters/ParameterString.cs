using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterString(string name, string raw, string value, bool isValid, string validationError) : AbstractParameterValue<string>(name, raw, value, isValid, validationError)
    {
    }
}
