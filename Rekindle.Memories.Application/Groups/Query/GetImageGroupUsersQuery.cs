using MediatR;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Application.Groups.Models;

namespace Rekindle.Memories.Application.Groups.Query;

public record GetImageGroupUsersQuery(Guid GroupId) : IRequest<IEnumerable<ImageGroupUserDto>>;

public class GetImageGroupUsersQueryHandler : IRequestHandler<GetImageGroupUsersQuery, IEnumerable<ImageGroupUserDto>>
{
    private readonly IGroupRepository _groupRepository;

    public GetImageGroupUsersQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<IEnumerable<ImageGroupUserDto>> Handle(GetImageGroupUsersQuery request,
        CancellationToken cancellationToken)
    {
        var group = await _groupRepository.FindByIdAsync(request.GroupId, cancellationToken);
        if (group == null)
        {
            return [];
        }

        return group.Members.Select(m => new ImageGroupUserDto(m.Id, m.LastFaceFileId, false))
            .Concat(group.TempUsers.Select(m => new ImageGroupUserDto(m.Id, m.LastFaceFileId, true)));
    }
}