using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command
{
    /// <summary>
    /// Implements the more generic parts of the ITextIoContext
    /// Allows for the implementation to handle the context specific parts
    /// </summary>
    /// <remarks>
    /// constructor requires a name and optional parent guid
    /// </remarks>
    /// <param name="name"></param>
    /// <param name="parentId"></param>
    public abstract class AbstractTextIo(string name, Guid? parentId = default) : ITextIoContext
    {
        public bool Verbose { get; set; } = false;

        public Guid Id { get; } = Guid.NewGuid();

        public string Name { get; set; } = name;

        public Guid? Parent { get; protected set; } = parentId;

        public bool HasPipedInput { get; private set; } = false;

        public string[] Parameters { get; set; } = Array.Empty<string>();

        protected ChannelReader<string>? inputPipe;
        protected ChannelWriter<string>? outputPipe;
        /// <summary>
        /// implementation must set the expected child's properties and pass environment values
        /// </summary>
        /// <param name="childArguments"></param>
        /// <returns></returns>
        public abstract Task<ITextIoContext> GetChild(string[]? childArguments = null);
        /// <summary>
        /// handles the Channel output and allows the implementation to handle 
        /// the final output
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual Task OutputChunk(string message)
        {
            if (this.outputPipe == null)
            {
                return this.HandleOutputChunk(message);
            }
            return outputPipe.WriteAsync(message).AsTask();
        }
        /// <summary>
        /// allow the implementation to handle the final output
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public abstract Task HandleOutputChunk(string chunk);
        /// <summary>
        /// allow the implementation to handle the prompting for input
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public abstract Task<string> PromptForCommand(string prompt);
        /// <summary>
        /// handles the Channel so the command just handles the await foreach
        /// </summary>
        /// <returns></returns>
        public async IAsyncEnumerable<string> ReadInputPipeChunks()
        {
            if (inputPipe == null) yield break;

            await foreach (var item in inputPipe.ReadAllAsync())
            {
                yield return item;
            }
        }
        /// <summary>
        /// set channel reader for pipeline
        /// </summary>
        /// <param name="reader"></param>
        public void SetInputPipe(ChannelReader<string> reader)
        {
            this.HasPipedInput = true;
            this.inputPipe = reader;
        }
        /// <summary>
        /// set channel writer for pipeline
        /// </summary>
        /// <param name="writer"></param>
        public void SetOutputPipe(ChannelWriter<string> writer)
        {
            this.outputPipe = writer;
        }
        /// <summary>
        /// display progress
        /// </summary>
        /// <param name="total"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public abstract Task<int> SetProgress(int total, int step);
        /// <summary>
        /// display status message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public abstract Task SetStatusMessage(string message);
        /// <summary>
        /// complete the output pipe
        /// </summary>
        /// <returns></returns>
        public ValueTask DisposeAsync()
        {
            this.Complete().Wait();
            return ValueTask.CompletedTask;
        }

        public Task Complete(string? message = null)
        {
            if (!String.IsNullOrEmpty(message)) this.SetStatusMessage(message).Wait();

            this.outputPipe?.TryComplete();
            return Task.CompletedTask;
        }

        public virtual Task AddTraceMessage(string message)
        {
            if (this.Verbose)
            {
                return this.OutputChunk("\tTRACE" + message);
            }
            // if we are not verbose, send the output to DEBUG
            System.Diagnostics.Debug.WriteLine(message);
            return Task.CompletedTask;
        }
        /// <summary>
        /// Thread safe collection of env vars
        /// MUST be set when creating a child!
        /// </summary>
        protected ConcurrentDictionary<string, string> EnvironmentVariables { get; set; } = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// <see cref="Xcaciv.Command.Interface.IEnvironment"/>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="addValue"></param>
        /// <returns></returns>
        public void SetValue(string key, string addValue)
        {
            // make case insensitive var names
            key = key.ToUpper();
            EnvironmentVariables.AddOrUpdate(key, addValue, (key, value) =>
            {
                AddTraceMessage($"Environment value {key} changed from {value} to {addValue}.").Wait();
                return addValue;
            });
        }
        /// <summary>
        /// <see cref="Xcaciv.Command.Interface.IEnvironment"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            // make case insensitive var names
            key = key.ToUpper();

            string? returnValue;
            EnvironmentVariables.TryGetValue(key, out returnValue);
            return returnValue ?? String.Empty;
        }
    }
}
