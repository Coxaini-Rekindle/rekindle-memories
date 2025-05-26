namespace Rekindle.Memories.Contracts;

/// <summary>
/// Base interface for all events in the system
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// When the event occurred (UTC)
    /// </summary>
    DateTime OccurredOn { get; }
}

/// <summary>
/// Base class for all events to ensure consistency
/// </summary>
public abstract class Event : IEvent
{
    /// <summary>
    /// Unique identifier for the event
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
    
    /// <summary>
    /// When the event occurred (UTC)
    /// </summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
