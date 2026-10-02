namespace ATRIUM.Web.Services;

public sealed class ObjectStorageOptions
{
    public string ServiceUrl { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;
}