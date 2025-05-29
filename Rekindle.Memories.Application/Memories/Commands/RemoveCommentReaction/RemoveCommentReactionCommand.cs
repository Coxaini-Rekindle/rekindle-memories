using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Commands.RemoveCommentReaction;

public record RemoveCommentReactionCommand : IRequest<ReactionSummaryDto>
{
    public Guid CommentId { get; init; }
    public Guid UserId { get; init; }
}
