using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.UserGroups.Contracts.GroupEvents;

namespace Rekindle.Memories.Application.Groups.EventHandlers;

public class UserLeftGroupEventHandler : IHandleMessages<UserLeftGroupEvent>
{
    private readonly IGroupRepository _groupRepository;

    public UserLeftGroupEventHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task Handle(UserLeftGroupEvent message)
    {
        var group = await _groupRepository.FindByIdAsync(message.GroupId);

        if (group == null)
        {
            // Handle the case where the group does not exist
            return;
        }

        group.RemoveMember(message.UserId);

        await _groupRepository.ReplaceAsync(group);
    }
}