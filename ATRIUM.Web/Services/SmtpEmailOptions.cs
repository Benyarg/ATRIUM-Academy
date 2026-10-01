namespace ATRIUM.Web.Services;

public sealed class SmtpEmailOptions
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "ATRIUM Academy";

    public string Username { get; set; } = string.Empty;

    public string AppPassword { get; set; } = string.Empty;

    public string RecipientEmail { get; set; } = string.Empty;
}