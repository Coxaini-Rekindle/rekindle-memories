namespace Rekindle.Memories.Infrastructure.DataAccess;

public sealed class FaceDatabaseConfig
{
    public string ConnectionString { get; init; } = null!;
    public string DatabaseName { get; init; } = null!;
}