using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Groups.Abstractions.Repositories;

public interface IGroupRepository
{
    Task InsertAsync(Group group, CancellationToken ctx = default);
    Task<Group?> FindByIdAsync(Guid groupId, CancellationToken ctx = default);
    Task<IEnumerable<Group>> FindByUserIdAsync(Guid userId, CancellationToken ctx = default);
    Task ReplaceAsync(Group group, CancellationToken ctx = default);
    Task ReplaceManyAsync(IEnumerable<Group> groups, CancellationToken ctx = default);
    Task ReplaceUserInGroupAsync(Guid groupId, User user, CancellationToken ctx = default);
}