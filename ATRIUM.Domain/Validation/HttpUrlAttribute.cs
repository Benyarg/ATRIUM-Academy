using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Domain.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class HttpUrlAttribute : ValidationAttribute
{
    public HttpUrlAttribute()
    {
        ErrorMessage = "Ingresa una URL válida que comience con http:// o https://.";
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return true;

        return HttpUrlRules.IsSafe(text);
    }
}

public static class HttpUrlRules
{
    public static bool IsSafe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
        {
            return false;
        }

        return string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            || string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
    }
}
