using MediatR;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Queries.GetPostById;

public record GetPostByIdQuery(
    Guid PostId,
    Guid UserId
) : IRequest<PostDto>;