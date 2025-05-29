using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Abstractions.Repositories;
using Rekindle.Memories.Application.Memories.Exceptions;
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

    public GetMemoryByIdQueryHandler(
        IMemoryRepository memoryRepository,
        IGroupRepository groupRepository)
    {
        _memoryRepository = memoryRepository;
        _groupRepository = groupRepository;
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
        }

        return new MemoryDto
        {
            Id = memory.Id,
            GroupId = memory.GroupId,
            Title = memory.Title,
            Description = memory.Description,
            CreatedAt = memory.CreatedAt,
            CreatorUserId = memory.CreatorUserId,
            ParticipantsIds = memory.ParticipantsIds,
            MainPostId = memory.MainPostId
        };
    }
}
