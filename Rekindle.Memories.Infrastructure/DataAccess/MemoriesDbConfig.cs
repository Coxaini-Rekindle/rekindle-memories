namespace Rekindle.Memories.Infrastructure.DataAccess;

public sealed class MemoriesDbConfig
{
    public const string MemoriesDb = "MemoriesDb";
    public string ConnectionString { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
}