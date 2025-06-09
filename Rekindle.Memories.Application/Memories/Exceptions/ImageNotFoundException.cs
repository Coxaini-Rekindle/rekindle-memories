using System.Net;
using Rekindle.Exceptions;

namespace Rekindle.Memories.Application.Memories.Exceptions;

public class ImageNotFoundException : AppException
{
    public ImageNotFoundException() : base(
        "Image not found",
        HttpStatusCode.NotFound,
        nameof(ImageNotFoundException))
    {
    }
}