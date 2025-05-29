using System.ComponentModel.DataAnnotations;

namespace Rekindle.Memories.Api.Models;

/// <summary>
/// Form data model for creating posts with file uploads
/// </summary>
public class CreatePostFormRequest
{
    [Required]
    public string MemoryId { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Image files to upload
    /// </summary>
    public List<IFormFile> Images { get; set; } = [];
    
    /// <summary>
    /// JSON string containing participant IDs for each image (array of arrays)
    /// Example: "[[\"guid1\",\"guid2\"],[\"guid3\"]]"
    /// </summary>
    public string? ParticipantIds { get; set; }
    
    /// <summary>
    /// Existing file IDs to include (JSON array)
    /// Example: "[\"fileId1\",\"fileId2\"]"
    /// </summary>
    public string? ExistingFileIds { get; set; }
}
