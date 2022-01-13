using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface;

/// <summary>
/// generic class for creating an instance of a command
/// </summary>
public interface ICommandFactory
{
    /// <summary>
    /// new up a instance of a particular command and box it
    /// </summary>
    /// <returns></returns>
    ICommand CreateCommandInstance();
}
