
namespace Xcaciv.Command.Interface
{
    public interface IAuditMaskingConfiguration
    {
        ISet<string> RedactedParameterNames { get; init; }
        ISet<string> RedactedParameterPatterns { get; init; }
        string RedactionPlaceholder { get; init; }

        string[] ApplyMasking(string[] parameters);
        bool ShouldRedact(string parameterName);
    }
}