using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.UserGroups.Contracts.GroupEvents;

namespace Rekindle.Memories.Application.Groups.EventHandlers;

public class GroupUpdatedEventHandler : IHandleMessages<GroupUpdatedEvent>
{
    private readonly IGroupRepository _groupRepository;

    public GroupUpdatedEventHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task Handle(GroupUpdatedEvent message)
    {
        var group = await _groupRepository.FindById(message.GroupId);
        
        if (group == null)
        {
            // Handle the case where the group does not exist
            return;
        }
        
        group.UpdateDetails(message.Name, message.Description);

        await _groupRepository.ReplaceGroup(group);
    }
}