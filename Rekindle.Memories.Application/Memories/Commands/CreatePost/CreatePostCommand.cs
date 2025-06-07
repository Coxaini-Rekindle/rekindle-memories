using MediatR;
using Rekindle.Memories.Application.Memories.Models;
using Rekindle.Memories.Application.Memories.Requests;

namespace Rekindle.Memories.Application.Memories.Commands.CreatePost;

public record CreatePostCommand(
    Guid MemoryId,
    string Content,
    List<CreateImageRequest> Images,
    Guid UserId
) : IRequest<PostDto>;