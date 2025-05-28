using Rebus.Handlers;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.UserGroups.Contracts.UserEvents;

namespace Rekindle.Memories.Application.Groups.EventHandlers;

public class UserAvatarChangedEventHandler : IHandleMessages<UserAvatarChangedEvent>
{
    private readonly IGroupRepository _groupRepository;

    public UserAvatarChangedEventHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task Handle(UserAvatarChangedEvent message)
    {
        var groups = await _groupRepository.FindByUserId(message.UserId);
        
        if (!groups.Any())
        {
            return;
        }

        foreach (var group in groups)
        {
            group.UpdateUserAvatar(message.UserId, message.AvatarFileId);
        }

        await _groupRepository.ReplaceGroups(groups);
    }
}
