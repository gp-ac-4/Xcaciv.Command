using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xcaciv.Command.Interface
{
    public record CommandResult
    {
        public bool IsSuccess { get; init; }
        public string? ErrorMessage { get; init; }
        public Exception? Exception { get; init; }
        public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
        public string? Output { get; init; }
    }
}
