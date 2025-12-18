using System;
using System.Collections.Generic;
using System.Linq;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;
using Xcaciv.Command.Interface.Exceptions;

namespace Xcaciv.Command;

/// <summary>
/// Handles discovery of commands from verified package directories.
/// </summary>
public class CommandLoader : ICommandLoader
{
    private readonly ICrawler _crawler;
    private readonly IVerfiedSourceDirectories _verifiedDirectories;

    public CommandLoader(ICrawler crawler, IVerfiedSourceDirectories verifiedDirectories)
    {
        _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
        _verifiedDirectories = verifiedDirectories ?? throw new ArgumentNullException(nameof(verifiedDirectories));
    }

    public IReadOnlyList<string> Directories => _verifiedDirectories.Directories;

    public void AddPackageDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory)) throw new ArgumentException("Directory is required", nameof(directory));
        _verifiedDirectories.AddDirectory(directory);
    }

    public void SetRestrictedDirectory(string restrictedDirectory)
    {
        if (string.IsNullOrWhiteSpace(restrictedDirectory)) throw new ArgumentException("Restricted directory is required", nameof(restrictedDirectory));
        _verifiedDirectories.SetRestrictedDirectory(restrictedDirectory);
    }

    public void LoadCommands(string subDirectory, Action<ICommandDescription> registerCommand)
    {
        if (registerCommand == null) throw new ArgumentNullException(nameof(registerCommand));

        if (_verifiedDirectories.Directories.Count == 0)
        {
            throw new NoPluginsFoundException("No base package directory configured. (Did you set the restricted directory?)");
        }

        foreach (var directory in _verifiedDirectories.Directories)
        {
            var packages = _crawler.LoadPackageDescriptions(directory, subDirectory);
            foreach (var command in packages.SelectMany(o => o.Value.Commands))
            {
                registerCommand(command.Value);
            }
        }
    }
}
