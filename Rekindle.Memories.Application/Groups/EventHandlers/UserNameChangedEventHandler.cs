using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.UserGroups.Contracts.UserEvents;

namespace Rekindle.Memories.Application.Groups.EventHandlers;

public class UserNameChangedEventHandler : IHandleMessages<UserNameChangedEvent>
{
    private readonly IGroupRepository _groupRepository;

    public UserNameChangedEventHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task Handle(UserNameChangedEvent message)
    {
        var groups = await _groupRepository.FindByUserId(message.UserId);
        
        if (!groups.Any())
        {
            return;
        }

        foreach (var group in groups)
        {
            group.UpdateUserName(message.UserId, message.NewName);
        }

        await _groupRepository.ReplaceGroups(groups);
    }
}
