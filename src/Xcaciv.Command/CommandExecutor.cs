using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command;

/// <summary>
/// Executes commands and handles help routing, audit logging, and environment updates.
/// </summary>
public class CommandExecutor : ICommandExecutor
{
    private readonly ICommandRegistry _registry;
    private readonly ICommandFactory _commandFactory;
    private IAuditLogger _auditLogger = new NoOpAuditLogger();

    public CommandExecutor(ICommandRegistry registry, ICommandFactory commandFactory)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
    }

    public string HelpCommand { get; set; } = "HELP";

    public IAuditLogger AuditLogger
    {
        get => _auditLogger;
        set => _auditLogger = value ?? new NoOpAuditLogger();
    }

    public async Task ExecuteAsync(string commandKey, IIoContext ioContext, IEnvironmentContext environmentContext)
    {
        if (commandKey == null) throw new ArgumentNullException(nameof(commandKey));
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));
        if (environmentContext == null) throw new ArgumentNullException(nameof(environmentContext));

        if (_registry.TryGetCommand(commandKey, out var commandDescription))
        {
            if (commandDescription == null)
            {
                await ioContext.AddTraceMessage($"Command registry returned null description for key: {commandKey}").ConfigureAwait(false);
                await ioContext.OutputChunk($"Command [{commandKey}] not found.").ConfigureAwait(false);
                return;
            }

            var parameters = ioContext.Parameters;
            if (parameters is { Length: > 0 } && parameters[0].Equals($"--{HelpCommand}", StringComparison.CurrentCultureIgnoreCase))
            {
                // For root commands with sub-commands, show one-line summaries; for leaf commands, show full help.
                if (commandDescription.SubCommands.Count > 0)
                {
                    await OutputOneLineHelp(commandDescription, ioContext).ConfigureAwait(false);
                }
                else
                {
                    var commandInstance = _commandFactory.CreateCommand(commandDescription, ioContext);
                    await ioContext.OutputChunk(commandInstance.Help(ioContext.Parameters, environmentContext)).ConfigureAwait(false);
                }
                return;
            }

            await ExecuteCommandWithErrorHandling(commandDescription, ioContext, environmentContext, commandKey).ConfigureAwait(false);
            return;
        }

        if (commandKey == HelpCommand)
        {
            await GetHelpAsync(string.Empty, ioContext, environmentContext).ConfigureAwait(false);
        }
        else
        {
            var message = $"Command [{commandKey}] not found.";
            await ioContext.OutputChunk($"{message} Try '{HelpCommand}'").ConfigureAwait(false);
            await ioContext.AddTraceMessage(message).ConfigureAwait(false);
        }
    }

    public Task GetHelpAsync(string command, IIoContext ioContext, IEnvironmentContext environmentContext)
    {
        if (ioContext == null) throw new ArgumentNullException(nameof(ioContext));
        if (environmentContext == null) throw new ArgumentNullException(nameof(environmentContext));

        return string.IsNullOrEmpty(command)
            ? OutputAllCommands(ioContext)
            : OutputCommandHelp(command, ioContext, environmentContext);
    }

    private async Task OutputAllCommands(IIoContext context)
    {
        foreach (var description in _registry.GetAllCommands())
        {
            await OutputOneLineHelp(description, context).ConfigureAwait(false);
        }
    }

    private async Task OutputCommandHelp(string command, IIoContext context, IEnvironmentContext env)
    {
        try
        {
            var commandKey = CommandDescription.GetValidCommandName(command);
            if (_registry.TryGetCommand(commandKey, out var description) && description != null)
            {
                var commandInstance = _commandFactory.CreateCommand(description, context);
                await context.OutputChunk(commandInstance.Help(context.Parameters, env)).ConfigureAwait(false);
            }
            else
            {
                await context.OutputChunk($"Command [{commandKey}] not found.").ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            await context.AddTraceMessage(
                $"Error getting help for command '{command}': {ex}").ConfigureAwait(false);
            var exceptionTypeName = ex.GetType().Name;
            await context.OutputChunk(
                $"Error getting help for command '{command}' ({exceptionTypeName}: {ex.Message}). See trace for more details.")
                .ConfigureAwait(false);
        }
    }

    private async Task ExecuteCommandWithErrorHandling(
        ICommandDescription commandDescription,
        IIoContext ioContext,
        IEnvironmentContext environmentContext,
        string commandKey)
    {
        var startTime = DateTime.UtcNow;
        var success = false;
        string? errorMessage = null;
        var resultFailures = new List<string>();

        try
        {
            await ioContext.AddTraceMessage($"ExecuteCommand: {commandKey} Start.").ConfigureAwait(false);

            var commandInstance = _commandFactory.CreateCommand(commandDescription, ioContext);

            await using (var childEnv = await environmentContext.GetChild(ioContext.Parameters).ConfigureAwait(false))
            {
                await foreach (var result in commandInstance.Main(ioContext, childEnv).ConfigureAwait(false))
                {
                    if (result == null)
                    {
                        continue;
                    }

                    if (result.IsSuccess)
                    {
                        if (!string.IsNullOrEmpty(result.Output))
                        {
                            await ioContext.OutputChunk(result.Output).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        var failureMessage = result.ErrorMessage ?? $"Command [{commandKey}] reported failure (CorrelationId: {result.CorrelationId}).";
                        resultFailures.Add(failureMessage);
                        await ioContext.OutputChunk(failureMessage).ConfigureAwait(false);

                        if (result.Exception != null)
                        {
                            await ioContext.AddTraceMessage(result.Exception.ToString()).ConfigureAwait(false);
                        }
                    }
                }

                if (commandDescription.ModifiesEnvironment && childEnv.HasChanged)
                {
                    environmentContext.UpdateEnvironment(childEnv.GetEnvironment());
                }
            }

            success = resultFailures.Count == 0;
        }
        catch (Exception ex)
        {
            success = false;
            errorMessage = ex.Message;
            await ioContext.OutputChunk($"Error executing {commandKey} (see trace for more info)").ConfigureAwait(false);
            await ioContext.SetStatusMessage("**Error: " + ex.Message).ConfigureAwait(false);
            await ioContext.AddTraceMessage(ex.ToString()).ConfigureAwait(false);
        }
        finally
        {
            await ioContext.AddTraceMessage($"ExecuteCommand: {commandKey} Done.").ConfigureAwait(false);

            if (!success && string.IsNullOrEmpty(errorMessage) && resultFailures.Count > 0)
            {
                errorMessage = string.Join(Environment.NewLine, resultFailures);
            }

            var duration = DateTime.UtcNow - startTime;
            _auditLogger?.LogCommandExecution(
                commandKey,
                ioContext.Parameters ?? Array.Empty<string>(),
                startTime,
                duration,
                success,
                errorMessage);
        }
    }

    private async Task OutputOneLineHelp(ICommandDescription description, IIoContext context)
    {
        if (string.IsNullOrEmpty(description.FullTypeName))
        {
            if (description.SubCommands.Count > 0)
            {
                var subCmd = _commandFactory.CreateCommand(description.SubCommands.First().Value, context);

                if (Attribute.GetCustomAttribute(subCmd.GetType(), typeof(CommandRootAttribute)) is CommandRootAttribute rootAttribute)
                {
                    await context.OutputChunk($"{rootAttribute.Command,-12} {rootAttribute.Description}").ConfigureAwait(false);
                }

                foreach (var subCommand in description.SubCommands)
                {
                    await OutputOneLineHelpFromInstance(subCommand.Value, context).ConfigureAwait(false);
                }
            }
            else
            {
                await context.AddTraceMessage($"No type name registered for command: {description.BaseCommand}").ConfigureAwait(false);
            }
        }
        else
        {
            await OutputOneLineHelpFromInstance(description, context).ConfigureAwait(false);
        }
    }

    private async Task OutputOneLineHelpFromInstance(ICommandDescription description, IIoContext context)
    {
        var commandInstance = _commandFactory.CreateCommand(description, context);
        await context.OutputChunk(commandInstance.OneLineHelp(context.Parameters)).ConfigureAwait(false);
    }
}
