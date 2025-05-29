namespace Rekindle.Memories.Application.Common.Abstractions;

/// <summary>
/// Provides transaction management capabilities
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Executes the provided function within a transaction
    /// </summary>
    /// <param name="operation">The operation to execute within the transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation result</returns>
    Task<T> ExecuteInTransactionAsync<T>(Func<ITransactionContext, Task<T>> operation, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes the provided action within a transaction
    /// </summary>
    /// <param name="operation">The operation to execute within the transaction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task representing the operation</returns>
    Task ExecuteInTransactionAsync(Func<ITransactionContext, Task> operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a transaction context that can be passed to repository operations
/// </summary>
public interface ITransactionContext
{
    /// <summary>
    /// Gets a value indicating whether this context represents an active transaction
    /// </summary>
    bool IsInTransaction { get; }
}
