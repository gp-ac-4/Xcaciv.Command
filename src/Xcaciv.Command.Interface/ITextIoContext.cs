using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface;

/// <summary>
/// String pump for UI syncronization context.
/// This interface is used to abstract the UI from the command processor.
/// Implementations shoudl be thread safe. They should handle pipeline input and 
/// output via ChannelReader and ChannelWriter. The implementation should default
/// to a particular bound output, but should prioritise the pipeline when channels
/// are available.
/// </summary>
public interface ITextIoContext : IEnvironment, IInputContext, IOutputContext, IAsyncDisposable
{
    /// <summary>
    /// current message context identifier
    /// </summary>
    Guid Id { get; }
    /// <summary>
    /// friendly name for user output
    /// </summary>
    string Name { get; }
    /// <summary>
    /// parent context identifier if there is one
    /// </summary>
    Guid? Parent { get; }
    /// <summary>
    /// create a child output context
    /// MUST pass down expected Environment values
    /// may track the instance for later use
    /// </summary>
    /// <param name="childArguments">arguments to pass to child context</param>
    /// <param name="pipeline">specify we are dealing with a pipeline</param>
    /// <returns></returns>
    Task<ITextIoContext> GetChild(string[]? childArguments = null);
}
