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

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "avatars" // Optional: folder in Cloudinary
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl.AbsoluteUri; // Return public URL
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("publicId is required");

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
            throw new ArgumentException("URL cannot be empty");

        var marker = "/upload/";
        var startIndex = url.IndexOf(marker);
        if (startIndex == -1)
            throw new ArgumentException("URL format is invalid");

        startIndex += marker.Length;

        var versionEndIndex = url.IndexOf('/', startIndex);
        if (versionEndIndex == -1)
            throw new ArgumentException("URL format is invalid");

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
