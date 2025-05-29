using MediatR;

namespace Rekindle.Memories.Application.Memories.Commands.DeletePost;

public record DeletePostCommand(
    Guid PostId,
    Guid UserId
) : IRequest;