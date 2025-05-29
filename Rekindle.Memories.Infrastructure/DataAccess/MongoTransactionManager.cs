using MongoDB.Driver;
using Rekindle.Memories.Application.Common.Abstractions;
using Rekindle.Memories.Infrastructure.DataAccess;

namespace Rekindle.Memories.Infrastructure.DataAccess;

/// <summary>
/// MongoDB implementation of transaction context
/// </summary>
public class MongoTransactionContext : ITransactionContext
{
    public IClientSessionHandle? Session { get; }

    public MongoTransactionContext(IClientSessionHandle? session = null)
    {
        Session = session;
    }

    public bool IsInTransaction => Session != null;
}

/// <summary>
/// MongoDB implementation of transaction manager
/// </summary>
public class MongoTransactionManager : ITransactionManager
{
    private readonly IMemoriesDbContext _dbContext;

    public MongoTransactionManager(IMemoriesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<ITransactionContext, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        using var session = await _dbContext.Client.StartSessionAsync(cancellationToken: cancellationToken);
        
        return await session.WithTransactionAsync(async (sess, ct) =>
        {
            var transactionContext = new MongoTransactionContext(sess);
            return await operation(transactionContext);
        }, cancellationToken: cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(Func<ITransactionContext, Task> operation, CancellationToken cancellationToken = default)
    {
        using var session = await _dbContext.Client.StartSessionAsync(cancellationToken: cancellationToken);
        
        await session.WithTransactionAsync(async (sess, ct) =>
        {
            var transactionContext = new MongoTransactionContext(sess);
            await operation(transactionContext);
            return true; // WithTransactionAsync requires a return value
        }, cancellationToken: cancellationToken);
    }
}
