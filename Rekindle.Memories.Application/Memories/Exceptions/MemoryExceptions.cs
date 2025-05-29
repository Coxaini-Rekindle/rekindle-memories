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

public class GroupNotFoundException : AppException
{
    public GroupNotFoundException() : base(
        "Group not found",
        HttpStatusCode.NotFound,
        nameof(GroupNotFoundException))
    {
    }
}

public class UserNotGroupMemberException : AppException
{
    public UserNotGroupMemberException() : base(
        "User is not a member of this group",
        HttpStatusCode.Forbidden,
        nameof(UserNotGroupMemberException))
    {
    }
}

public class PostNotFoundException : AppException
{
    public PostNotFoundException() : base(
        "Post not found",
        HttpStatusCode.NotFound,
        nameof(PostNotFoundException))
    {
    }
}

public class CommentNotFoundException : AppException
{
    public CommentNotFoundException() : base(
        "Comment not found",
        HttpStatusCode.NotFound,
        nameof(CommentNotFoundException))
    {
    }
}
