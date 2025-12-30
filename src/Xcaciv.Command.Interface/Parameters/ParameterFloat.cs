using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterFloat(string name, string raw, float value, bool isValid, string validationError) : AbstractParameterValue<float>(name, raw, value, isValid, validationError)
    {
    }
}
