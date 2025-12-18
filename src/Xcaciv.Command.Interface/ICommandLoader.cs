using System;
using System.Collections.Generic;

namespace Xcaciv.Command.Interface;

/// <summary>
/// Discovers command packages from verified directories and registers them with the runtime.
/// </summary>
public interface ICommandLoader
{
    /// <summary>
    /// Gets the list of directories that will be scanned for command packages.
    /// </summary>
    IReadOnlyList<string> Directories { get; }

    /// <summary>
    /// Adds a directory to the list of verified package directories.
    /// </summary>
    /// <param name="directory">Directory path that contains command packages.</param>
    void AddPackageDirectory(string directory);

    /// <summary>
    /// Sets the restricted directory boundary that package directories must reside in.
    /// </summary>
    /// <param name="restrictedDirectory">Base directory allowed for plugin loading.</param>
    void SetRestrictedDirectory(string restrictedDirectory);

    /// <summary>
    /// Loads commands from the configured directories and invokes the supplied callback for each discovered command.
    /// </summary>
    /// <param name="subDirectory">Optional sub-folder that contains compiled command assemblies.</param>
    /// <param name="registerCommand">Callback used to register discovered commands.</param>
    void LoadCommands(string subDirectory, Action<ICommandDescription> registerCommand);
}
