using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Commands.UpdateComment;

public record UpdateCommentCommand(
    Guid CommentId,
    string Content,
    Guid UserId
) : IRequest<CommentDto>;
