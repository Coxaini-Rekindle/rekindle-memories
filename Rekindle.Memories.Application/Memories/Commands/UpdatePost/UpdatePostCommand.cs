using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Commands.UpdatePost;

public record UpdatePostCommand(
    Guid PostId,
    string? Content,
    Guid UserId
) : IRequest<PostDto>;