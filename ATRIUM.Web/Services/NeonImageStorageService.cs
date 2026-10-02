using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace ATRIUM.Web.Services;

public sealed class NeonImageStorageService : IImageStorageService
{
    private const long MaxFileSize = 5_000_000;

    private static readonly IReadOnlyDictionary<string, string[]>
        AllowedContentTypes =
            new Dictionary<string, string[]>(
                StringComparer.OrdinalIgnoreCase)
            {
                [".jpg"] = ["image/jpeg", "image/pjpeg"],
                [".jpeg"] = ["image/jpeg", "image/pjpeg"],
                [".png"] = ["image/png"],
                [".webp"] = ["image/webp"]
            };

    private readonly IAmazonS3 _s3;
    private readonly ObjectStorageOptions _options;

    public NeonImageStorageService(
        IAmazonS3 s3,
        IOptions<ObjectStorageOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<string?> ValidateAsync(
        IFormFile? file,
        bool required,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return required
                ? "Selecciona una imagen JPG, PNG o WEBP."
                : null;
        }

        if (file.Length > MaxFileSize)
        {
            return "La imagen no puede superar los 5 MB.";
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedContentTypes.TryGetValue(
                extension,
                out var allowedMimeTypes))
        {
            return "Formato no permitido. Usa JPG, PNG o WEBP.";
        }

        if (!allowedMimeTypes.Contains(
                file.ContentType,
                StringComparer.OrdinalIgnoreCase))
        {
            return "El tipo de contenido de la imagen no coincide con su extensión.";
        }

        if (!await HasValidSignatureAsync(
                file,
                extension,
                cancellationToken))
        {
            return "El archivo no contiene una imagen válida.";
        }

        return null;
    }

    public async Task<string> SaveAsync(
        IFormFile file,
        ImageStorageArea area,
        CancellationToken cancellationToken = default)
    {
        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        var folderName = GetFolderName(area);

        var fileName = $"{Guid.NewGuid():N}{extension}";

        var key = $"{folderName}/{fileName}";

        await using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        };

        await _s3.PutObjectAsync(
            request,
            cancellationToken);

        return BuildPublicUrl(key);
    }

    public async Task DeleteAsync(
        string? path,
        ImageStorageArea area,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var storageBaseUrl =
            $"{_options.ServiceUrl.TrimEnd('/')}/{_options.BucketName}/";

        if (!path.StartsWith(
                storageBaseUrl,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var key = path[storageBaseUrl.Length..];

        if (string.IsNullOrWhiteSpace(key))
            return;

        var expectedPrefix =
            $"{GetFolderName(area)}/";

        if (!key.StartsWith(
                expectedPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await _s3.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = _options.BucketName,
                Key = key
            },
            cancellationToken);
    }

    private string BuildPublicUrl(string key)
    {
        return
            $"{_options.ServiceUrl.TrimEnd('/')}/" +
            $"{_options.BucketName}/" +
            $"{key}";
    }

    private static string GetFolderName(
        ImageStorageArea area)
    {
        return area switch
        {
            ImageStorageArea.Courses => "cursos",
            ImageStorageArea.Carousel => "carousel",

            _ => throw new ArgumentOutOfRangeException(
                nameof(area),
                area,
                null)
        };
    }

    private static async Task<bool> HasValidSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[12];

        await using var stream =
            file.OpenReadStream();

        var bytesRead = await stream.ReadAsync(
            buffer.AsMemory(
                0,
                buffer.Length),
            cancellationToken);

        if (bytesRead < 4)
            return false;

        return extension switch
        {
            ".jpg" or ".jpeg" =>
                buffer[0] == 0xFF &&
                buffer[1] == 0xD8 &&
                buffer[2] == 0xFF,

            ".png" =>
                bytesRead >= 8 &&
                buffer[0] == 0x89 &&
                buffer[1] == 0x50 &&
                buffer[2] == 0x4E &&
                buffer[3] == 0x47 &&
                buffer[4] == 0x0D &&
                buffer[5] == 0x0A &&
                buffer[6] == 0x1A &&
                buffer[7] == 0x0A,

            ".webp" =>
                bytesRead >= 12 &&
                buffer[0] == (byte)'R' &&
                buffer[1] == (byte)'I' &&
                buffer[2] == (byte)'F' &&
                buffer[3] == (byte)'F' &&
                buffer[8] == (byte)'W' &&
                buffer[9] == (byte)'E' &&
                buffer[10] == (byte)'B' &&
                buffer[11] == (byte)'P',

            _ => false
        };
    }
}