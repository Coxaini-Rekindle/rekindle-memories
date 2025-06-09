using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class UserNotFoundException : AppException
{
    public UserNotFoundException() : base(
        "User not found",
        HttpStatusCode.NotFound,
        nameof(UserNotFoundException))
    {
    }
}
