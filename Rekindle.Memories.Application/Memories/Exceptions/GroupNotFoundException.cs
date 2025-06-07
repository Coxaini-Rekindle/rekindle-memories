using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class GroupNotFoundException : AppException
{
    public GroupNotFoundException() : base(
        "Group not found",
        HttpStatusCode.NotFound,
        nameof(GroupNotFoundException))
    {
    }
}