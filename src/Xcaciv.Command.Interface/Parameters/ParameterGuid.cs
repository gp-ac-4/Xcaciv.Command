using System;
using System.Collections.Generic;
using System.Text;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterGuid(string name, string raw, Guid value, bool isValid, string validationError) : AbstractParameterValue<Guid>(name, raw, value, isValid, validationError)
    {
    }
}
