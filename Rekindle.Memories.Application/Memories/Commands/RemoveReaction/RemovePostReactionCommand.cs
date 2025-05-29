using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Commands.RemoveReaction;

public record RemovePostReactionCommand : IRequest<ReactionSummaryDto>
{
    public Guid PostId { get; init; }
    public Guid UserId { get; init; }
}
