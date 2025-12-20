using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LostAndFound.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LostAndFound.Application.Services;

public class ImageUploadService : IImageUploadService
{
    private readonly Cloudinary _cloudinary;

    public ImageUploadService(IConfiguration configuration)
    {
        var cloudinarySettings = configuration.GetSection("Cloudinary");
        var cloudName = cloudinarySettings["CloudName"];
        var apiKey = cloudinarySettings["ApiKey"];
        var apiSecret = cloudinarySettings["ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string?> UploadImageAsync(Stream imageStream, string fileName)
    {
        try
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "lost-and-found", // Folder trong Cloudinary
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }

            throw new Exception($"Upload failed: {uploadResult.Error?.Message}");
        }
        catch (Exception)
        {
            return null; // Trả về null nếu upload thất bại
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        try
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };

            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.StatusCode == System.Net.HttpStatusCode.OK && result.Result == "ok";
        }
        catch
        {
            return false;
        }
    }
}

