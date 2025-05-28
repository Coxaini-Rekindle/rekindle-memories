using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Groups.Abstractions.Repositories;

public interface IGroupRepository
{
    Task InsertGroup(Group group, CancellationToken ctx = default);
    Task<Group?> FindById(Guid groupId, CancellationToken ctx = default);
    Task<IEnumerable<Group>> FindByUserId(Guid userId, CancellationToken ctx = default);
    Task ReplaceGroup(Group group, CancellationToken ctx = default);
    Task ReplaceGroups(IEnumerable<Group> groups, CancellationToken ctx = default);
    Task ReplaceUserInGroup(Guid groupId, User user, CancellationToken ctx = default);
}