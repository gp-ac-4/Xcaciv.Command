using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterDecimal(string name, string raw, decimal value, bool isValid, string validationError) : AbstractParameterValue<decimal>(name, raw, value, isValid, validationError)
    {
    }
}
