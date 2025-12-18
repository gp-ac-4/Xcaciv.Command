using System;

namespace Xcaciv.Command.Interface
{

    public sealed record CommandResult<T> : IResult<T>
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
        public T? Output { get; init; }

        public static CommandResult<T> Success(T? output) => new()
        {
            IsSuccess = true,
            Output = output
        };

        public static CommandResult<T> Failure(string? errorMessage = null, Exception? exception = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Exception = exception
        };
    }
}
