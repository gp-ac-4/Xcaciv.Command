namespace Xcaciv.Command.Interface
{
    public interface ICommandDescription
    {
        /// <summary>
        /// string used to identify the command
        /// </summary>
        string BaseCommand { get; }
        /// <summary>
        /// subcommnands that are registered in their own classes
        /// </summary>
        Dictionary<string, ICommandDescription> SubCommands { get; }
        /// <summary>
        /// base command type name
        /// type must implement ICommandDelegate
        /// </summary>
        string FullTypeName { get; }
        /// <summary>
        /// indicates the intent to modify the environment values
        /// this is only allowed for certain commands
        /// </summary>
        bool ModifiesEnvironment { get; }
        /// <summary>
        /// assembly information for loading the command
        /// </summary>
        PackageDescription PackageDescription { get; }
    }
}