using MongoDB.Driver;
using Rekindle.Memories.Application.Groups.Abstractions.Repositories;
using Rekindle.Memories.Domain;
using Rekindle.Memories.Infrastructure.DataAccess;

namespace Rekindle.Memories.Infrastructure.Repositories.Groups;

public class GroupRepository : IGroupRepository
{
    private readonly IMongoCollection<Group> _groupCollection;

    public GroupRepository(IMemoriesDbContext memoriesDbContext)
    {
        _groupCollection = memoriesDbContext.Groups;
    }

    public async Task InsertAsync(Group group, CancellationToken ctx)
    {
        await _groupCollection.InsertOneAsync(group, new InsertOneOptions(), ctx);
    }

    public async Task<Group?> FindByIdAsync(Guid groupId, CancellationToken ctx)
    {
        var filter = Builders<Group>.Filter.Eq(g => g.Id, groupId);
        return await _groupCollection.Find(filter).FirstOrDefaultAsync(ctx);
    }

    public async Task<IEnumerable<Group>> FindByUserIdAsync(Guid userId, CancellationToken ctx)
    {
        var filter = Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.Id == userId);
        return await _groupCollection.Find(filter).ToListAsync(ctx);
    }

    public async Task ReplaceAsync(Group group, CancellationToken ctx = default)
    {
        var filter = Builders<Group>.Filter.Eq(g => g.Id, group.Id);
        await _groupCollection.ReplaceOneAsync(filter, group, new ReplaceOptions(), ctx);
    }

    public async Task ReplaceManyAsync(IEnumerable<Group> groups, CancellationToken ctx = default)
    {
        var operations = new List<WriteModel<Group>>();

        foreach (var group in groups)
        {
            var filter = Builders<Group>.Filter.Eq(g => g.Id, group.Id);
            var replaceModel = new ReplaceOneModel<Group>(filter, group);
            operations.Add(replaceModel);
        }

        if (operations.Any())
        {
            await _groupCollection.BulkWriteAsync(operations, new BulkWriteOptions(), ctx);
        }
    }

    public async Task ReplaceUserInGroupAsync(Guid groupId, User user, CancellationToken ctx = default)
    {
        var filter = Builders<Group>.Filter.And(
            Builders<Group>.Filter.Eq(g => g.Id, groupId),
            Builders<Group>.Filter.ElemMatch(g => g.Members, m => m.Id == user.Id)
        );

        var update = Builders<Group>.Update.Set("Members.$", user);
        await _groupCollection.UpdateOneAsync(filter, update, new UpdateOptions(), ctx);
    }
}