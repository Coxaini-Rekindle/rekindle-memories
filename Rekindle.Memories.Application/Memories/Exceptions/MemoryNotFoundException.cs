using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class MemoryNotFoundException : AppException
{
    public MemoryNotFoundException() : base(
        "Memory not found",
        HttpStatusCode.NotFound,
        nameof(MemoryNotFoundException))
    {
    }
}