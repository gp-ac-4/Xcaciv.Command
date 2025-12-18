namespace Xcaciv.Command.Interface
{
    public interface IResult<out T>
    {
        bool IsSuccess { get; }
        string? ErrorMessage { get; }
        Exception? Exception { get; }
        string CorrelationId { get; }
        T? Output { get; }
    }
}
