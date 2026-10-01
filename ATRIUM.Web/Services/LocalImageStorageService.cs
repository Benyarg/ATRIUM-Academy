using Microsoft.AspNetCore.Http;

namespace ATRIUM.Web.Services;

public sealed class LocalImageStorageService : IImageStorageService
{
    private const long MaxFileSize = 5_000_000;

    private static readonly IReadOnlyDictionary<string, string[]> AllowedContentTypes =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ["image/jpeg", "image/pjpeg"],
            [".jpeg"] = ["image/jpeg", "image/pjpeg"],
            [".png"] = ["image/png"],
            [".webp"] = ["image/webp"]
        };

    private readonly IWebHostEnvironment _environment;

    public LocalImageStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string?> ValidateAsync(
        IFormFile? file,
        bool required,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return required ? "Selecciona una imagen JPG, PNG o WEBP." : null;

        if (file.Length > MaxFileSize)
            return "La imagen no puede superar los 5 MB.";

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedContentTypes.TryGetValue(extension, out var allowedMimeTypes))
            return "Formato no permitido. Usa JPG, PNG o WEBP.";

        if (!allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return "El tipo de contenido de la imagen no coincide con su extensión.";

        if (!await HasValidSignatureAsync(file, extension, cancellationToken))
            return "El archivo no contiene una imagen válida.";

        return null;
    }

    public async Task<string> SaveAsync(
        IFormFile file,
        ImageStorageArea area,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var folderName = GetFolderName(area);
        var folder = Path.Combine(_environment.WebRootPath, "img", folderName);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        try
        {
            await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await file.CopyToAsync(stream, cancellationToken);
        }
        catch
        {
            if (File.Exists(fullPath))
                File.Delete(fullPath);

            throw;
        }

        return $"/img/{folderName}/{fileName}";
    }

    public void Delete(string? relativePath, ImageStorageArea area)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return;

        var folderName = GetFolderName(area);
        var expectedPrefix = $"/img/{folderName}/";
        if (!relativePath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            return;

        var fileName = Path.GetFileName(relativePath);
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        var folder = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "img", folderName));
        var fullPath = Path.GetFullPath(Path.Combine(folder, fileName));

        if (!fullPath.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return;

        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    private static string GetFolderName(ImageStorageArea area) => area switch
    {
        ImageStorageArea.Courses => "cursos",
        ImageStorageArea.Carousel => "carousel",
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, null)
    };

    private static async Task<bool> HasValidSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[12];
        await using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

        if (bytesRead < 4)
            return false;

        return extension switch
        {
            ".jpg" or ".jpeg" => buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF,
            ".png" => bytesRead >= 8
                      && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47
                      && buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A,
            ".webp" => bytesRead >= 12
                       && buffer[0] == (byte)'R' && buffer[1] == (byte)'I' && buffer[2] == (byte)'F' && buffer[3] == (byte)'F'
                       && buffer[8] == (byte)'W' && buffer[9] == (byte)'E' && buffer[10] == (byte)'B' && buffer[11] == (byte)'P',
            _ => false
        };
    }
}
