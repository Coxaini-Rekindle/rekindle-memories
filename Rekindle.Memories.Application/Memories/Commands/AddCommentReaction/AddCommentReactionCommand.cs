using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Commands.AddCommentReaction;

public record AddCommentReactionCommand : IRequest<ReactionSummaryDto>
{
    public Guid CommentId { get; init; }
    public ReactionTypeDto ReactionType { get; init; }
    public Guid UserId { get; init; }
}
