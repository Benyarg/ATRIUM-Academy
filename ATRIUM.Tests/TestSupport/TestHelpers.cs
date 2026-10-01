using System.ComponentModel.DataAnnotations;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace ATRIUM.Tests.TestSupport;

internal static class TestHelpers
{
    public static AtriumDbContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AtriumDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"atrium-tests-{Guid.NewGuid():N}")
            .Options;

        return new AtriumDbContext(options);
    }

    public static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    public static bool HasErrorFor(IEnumerable<ValidationResult> results, string memberName) =>
        results.Any(result => result.MemberNames.Contains(memberName, StringComparer.Ordinal));

    public static T ConfigureController<T>(T controller) where T : Controller
    {
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());
        return controller;
    }

    public static FormFile CreateFormFile(byte[] bytes, string fileName, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    public static byte[] MinimalJpeg() => [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    public static byte[] MinimalPng() => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    public static byte[] MinimalWebp() =>
        [(byte)'R', (byte)'I', (byte)'F', (byte)'F', 0x00, 0x00, 0x00, 0x00, (byte)'W', (byte)'E', (byte)'B', (byte)'P'];
}

internal sealed class TestTempDataProvider : ITempDataProvider
{
    private IDictionary<string, object> _values = new Dictionary<string, object>();

    public IDictionary<string, object> LoadTempData(HttpContext context) =>
        new Dictionary<string, object>(_values);

    public void SaveTempData(HttpContext context, IDictionary<string, object> values) =>
        _values = new Dictionary<string, object>(values);
}

internal sealed class TestWebHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "ATRIUM.Tests";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = string.Empty;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
