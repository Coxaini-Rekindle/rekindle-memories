using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class UserNotGroupMemberException : AppException
{
    public UserNotGroupMemberException() : base(
        "User is not a member of this group",
        HttpStatusCode.Forbidden,
        nameof(UserNotGroupMemberException))
    {
    }
}