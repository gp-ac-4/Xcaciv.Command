using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterBool(string name, string raw, bool value, bool isValid, string validationError) : AbstractParameterValue<bool>(name, raw, value, isValid, validationError)
    {
    }
}
