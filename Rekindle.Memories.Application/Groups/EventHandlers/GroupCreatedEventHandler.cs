using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Domain;
using Rekindle.UserGroups.Contracts.GroupEvents;

namespace Rekindle.Memories.Application.Groups.EventHandlers;

public class GroupCreatedEventHandler : IHandleMessages<GroupCreatedEvent>
{
    private readonly IGroupRepository _groupRepository;

    public GroupCreatedEventHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public Task Handle(GroupCreatedEvent message)
    {
        var groupCreator = message.CreatedByUser;
        var creator = User.Create(groupCreator.Id, groupCreator.Name, groupCreator.UserName, groupCreator.AvatarFileId);
        var group = Group.Create(message.GroupId, message.Name, message.Description, creator);

        return _groupRepository.InsertGroup(group);
    }
}