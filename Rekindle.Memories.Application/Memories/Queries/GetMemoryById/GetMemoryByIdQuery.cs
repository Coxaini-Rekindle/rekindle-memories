using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
using Rekindle.Memories.Application.Memories.Mappings;
using Rekindle.Memories.Application.Memories.Models;

namespace Rekindle.Memories.Application.Memories.Queries.GetMemoryById;

public record GetMemoryByIdQuery : IRequest<MemoryDto>
{
    public Guid MemoryId { get; init; }
    public Guid UserId { get; init; }
}

public class GetMemoryByIdQueryHandler : IRequestHandler<GetMemoryByIdQuery, MemoryDto>
{
    private readonly IMemoryRepository _memoryRepository;
    private readonly IGroupRepository _groupRepository;
    private readonly IPostRepository _postRepository;

    public GetMemoryByIdQueryHandler(
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository,
        IPostRepository postRepository)
    {
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
        _postRepository = postRepository;
    }

    public async Task<MemoryDto> Handle(GetMemoryByIdQuery request, CancellationToken cancellationToken)
    {
        var memory = await _memoryRepository.FindById(request.MemoryId, cancellationToken);
        if (memory == null)
        {
            throw new MemoryNotFoundException();
        }

        // Check if user is a member of the group
        var group = await _groupRepository.FindById(memory.GroupId, cancellationToken);
        if (group == null)
        {
            throw new GroupNotFoundException();
        }

        var isUserMember = group.Members.Any(m => m.Id == request.UserId);
        if (!isUserMember)
        {
            throw new UserNotGroupMemberException();
        } // Fetch the main post

        var mainPost = await _postRepository.FindById(memory.MainPostId, cancellationToken);

        return memory.ToDto(mainPost);
    }
}