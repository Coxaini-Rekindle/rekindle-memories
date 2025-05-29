using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Commands.AddReaction;

public record AddPostReactionCommand : IRequest<ReactionSummaryDto>
{
    public Guid PostId { get; init; }
    public ReactionTypeDto ReactionType { get; init; }
    public Guid UserId { get; init; }
}
