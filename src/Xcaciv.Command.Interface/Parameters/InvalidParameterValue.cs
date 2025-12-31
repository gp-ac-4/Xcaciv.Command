namespace Xcaciv.Command.Interface.Parameters;

/// <summary>
/// Sentinel value representing a failed parameter conversion.
/// Used internally to maintain type consistency when conversion fails.
/// </summary>
public sealed class InvalidParameterValue
{
    private InvalidParameterValue() { }
    
    /// <summary>
    /// Singleton instance of the invalid parameter sentinel.
    /// </summary>
    public static readonly InvalidParameterValue Instance = new();
    
    public override string ToString() => "[Invalid Parameter Value]";
}