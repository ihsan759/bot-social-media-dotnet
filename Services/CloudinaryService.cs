using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> config)
    {
        var account = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new HttpException("File is empty", 400);

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder
        };

        try
        {
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new HttpException($"Cloudinary upload failed: {uploadResult.Error.Message}", 500);

            return uploadResult.SecureUrl?.AbsoluteUri
                   ?? throw new HttpException("Cloudinary upload failed: no URL returned", 500);
        }
        catch (Exception ex)
        {
            throw new HttpException("Image upload failed", 500, ex);
        }
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new HttpException("publicId is required", 400);

        var deletionParams = new DeletionParams(publicId)
        {
            Invalidate = true // Optional: invalidate the cached version
        };

        var result = await _cloudinary.DestroyAsync(deletionParams);

        return result.Result == "ok" || result.Result == "not found";
    }

    public string ExtractPublicId(string url)
    {
        if (string.IsNullOrEmpty(url))
            throw new HttpException("URL cannot be empty", 400);

        var marker = "/upload/";
        var startIndex = url.IndexOf(marker);
        if (startIndex == -1)
            throw new HttpException("URL format is invalid", 400);

        startIndex += marker.Length;

        var versionEndIndex = url.IndexOf('/', startIndex);
        if (versionEndIndex == -1)
            throw new HttpException("URL format is invalid", 400);

        var pathWithExtension = url.Substring(versionEndIndex + 1);

        var publicId = Path.ChangeExtension(pathWithExtension, null);

        return publicId.Replace("\\", "/");
    }
}

public class CloudinarySettings
{
    public string CloudName { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ApiSecret { get; set; } = "";
}
