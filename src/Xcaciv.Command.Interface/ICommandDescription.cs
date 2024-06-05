namespace Xcaciv.Command.Interface
{
    public interface ICommandDescription
    {
        string BaseCommand { get; set; }
        string FullTypeName { get; set; }
        bool ModifiesEnvironment { get; set; }
        PackageDescription PackageDescription { get; set; }
    }
}