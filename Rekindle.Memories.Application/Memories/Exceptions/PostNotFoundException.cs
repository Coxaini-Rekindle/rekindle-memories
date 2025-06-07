using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class PostNotFoundException : AppException
{
    public PostNotFoundException() : base(
        "Post not found",
        HttpStatusCode.NotFound,
        nameof(PostNotFoundException))
    {
    }
}