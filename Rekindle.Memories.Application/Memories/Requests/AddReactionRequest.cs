using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Requests;

public record AddReactionRequest
{
    public ReactionTypeDto Type { get; init; }
}