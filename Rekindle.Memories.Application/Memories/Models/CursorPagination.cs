namespace Rekindle.Memories.Application.Memories.Models;

public record CursorPaginationRequest
{
    public int Limit { get; init; } = 20;
    public DateTime? Cursor { get; init; }
}

public record CursorPaginationResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public DateTime? NextCursor { get; init; }
    public bool HasMore { get; init; }
}