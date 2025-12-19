using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xcaciv.Command.Core;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Attributes;

namespace Xcaciv.Command;

/// <summary>
/// In-memory registry that tracks available commands and their metadata.
/// Note: This registry is not thread-safe for concurrent writes. Register commands during
/// application startup or guard Add operations externally if multi-threaded registration is required.
/// </summary>
public class CommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, ICommandDescription> _commands = new();

    public void AddCommand(ICommandDescription command)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));

        if (command.SubCommands.Count > 0 &&
            _commands.TryGetValue(command.BaseCommand, out var existingDescription))
        {
            foreach (var subCommand in command.SubCommands)
            {
                existingDescription.SubCommands[subCommand.Key] = subCommand.Value;
            }
        }
        else
        {
            _commands[command.BaseCommand] = command;
        }
    }

    public void AddCommand(string packageKey, Type commandType, bool modifiesEnvironment = false)
    {
        if (commandType == null) throw new ArgumentNullException(nameof(commandType));

        if (Attribute.GetCustomAttribute(commandType, typeof(CommandRegisterAttribute)) is not CommandRegisterAttribute)
        {
            Trace.WriteLine(
                $"{commandType.FullName} implements ICommandDelegate but does not have CommandRegisterAttribute. Unable to automatically register.");
            return;
        }

        var packageDesc = new PackageDescription
        {
            Name = packageKey,
            FullPath = commandType.Assembly.Location
        };

        var commandDesc = CommandParameters.CreatePackageDescription(commandType, packageDesc);
        if (commandDesc is CommandDescription mutableDescription)
        {
            mutableDescription.ModifiesEnvironment = modifiesEnvironment;
        }
        AddCommand(commandDesc);
    }

    public void AddCommand(string packageKey, ICommandDelegate command, bool modifiesEnvironment = false)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));
        AddCommand(packageKey, command.GetType(), modifiesEnvironment);
    }

    public bool TryGetCommand(string commandKey, out ICommandDescription? commandDescription)
    {
        if (commandKey == null) throw new ArgumentNullException(nameof(commandKey));
        return _commands.TryGetValue(commandKey, out commandDescription);
    }

    public IEnumerable<ICommandDescription> GetAllCommands() => _commands.Values;
}
