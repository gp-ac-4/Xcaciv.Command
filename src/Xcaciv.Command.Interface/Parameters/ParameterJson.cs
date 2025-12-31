using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Xcaciv.Command.Interface.Parameters
{
    public class ParameterJson(string name, string raw, JsonElement value, bool isValid, string validationError) : AbstractParameterValue<JsonElement>(name, raw, value, isValid, validationError)
    {
    }
}
