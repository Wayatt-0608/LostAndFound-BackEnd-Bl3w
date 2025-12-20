namespace LostAndFound.Application.Interfaces;

public interface IImageUploadService
{
    Task<string?> UploadImageAsync(Stream imageStream, string fileName);
    Task<bool> DeleteImageAsync(string publicId);
}

