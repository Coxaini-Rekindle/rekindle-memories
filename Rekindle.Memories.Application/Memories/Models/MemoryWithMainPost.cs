using Rekindle.Memories.Domain;

namespace Rekindle.Memories.Application.Memories.Models;

public class MemoryWithMainPost
{
    public Memory Memory { get; set; } = null!;
    public Post? MainPost { get; set; }
}
