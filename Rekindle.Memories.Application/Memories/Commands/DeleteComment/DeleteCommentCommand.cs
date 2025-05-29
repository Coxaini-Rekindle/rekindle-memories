using MediatR;

namespace Rekindle.Memories.Application.Memories.Commands.DeleteComment;

public record DeleteCommentCommand(
    Guid CommentId,
    Guid UserId
) : IRequest;
