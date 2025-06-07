using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class CommentNotFoundException : AppException
{
    public CommentNotFoundException() : base(
        "Comment not found",
        HttpStatusCode.NotFound,
        nameof(CommentNotFoundException))
    {
    }
}