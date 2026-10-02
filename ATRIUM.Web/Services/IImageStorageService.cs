using Microsoft.AspNetCore.Http;

namespace ATRIUM.Web.Services;

public enum ImageStorageArea
{
    Courses,
    Carousel
}

public interface IImageStorageService
{
    Task<string> SaveAsync(
        IFormFile file,
        ImageStorageArea area,
        CancellationToken cancellationToken = default);

    Task<string?> ValidateAsync(
        IFormFile? file,
        bool required,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string? path,
        ImageStorageArea area,
        CancellationToken cancellationToken = default);
}