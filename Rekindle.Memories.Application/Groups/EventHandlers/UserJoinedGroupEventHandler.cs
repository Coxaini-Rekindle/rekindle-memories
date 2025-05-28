using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.UserGroups.Contracts.GroupEvents;

namespace Rekindle.Memories.Application.Groups.EventHandlers;

public class UserJoinedGroupEventHandler : IHandleMessages<UserJoinedGroupEvent>
{
    private readonly IGroupRepository _groupRepository;

    public UserJoinedGroupEventHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task Handle(UserJoinedGroupEvent message)
    {
        var group = await _groupRepository.FindById(message.GroupId);
        
        if (group == null)
        {
            // Handle the case where the group does not exist, e.g., log an error or throw an exception
            return;
        }
        
        group.AddMember(message.UserId, message.Name, message.UserName, message.AvatarFileId);

        await _groupRepository.ReplaceGroup(group);
    }
}