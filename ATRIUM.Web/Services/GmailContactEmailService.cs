using ATRIUM.Domain.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;

namespace ATRIUM.Web.Services;

public class GmailContactEmailService : IContactEmailService
{
    private readonly SmtpEmailOptions _options;
    private readonly ILogger<GmailContactEmailService> _logger;

    public GmailContactEmailService(
        IOptions<SmtpEmailOptions> options,
        ILogger<GmailContactEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendContactNotificationAsync(
        ConsultaContacto consulta,
        CancellationToken cancellationToken = default)
    {
        ValidateConfiguration();

        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                _options.SenderName,
                _options.SenderEmail));

        email.To.Add(
            MailboxAddress.Parse(_options.RecipientEmail));

        /*
         * Esto permite que al presionar "Responder" desde Gmail,
         * la respuesta se dirija al usuario que envió la consulta.
         */
        email.ReplyTo.Add(
            new MailboxAddress(
                consulta.Nombre,
                consulta.Email));

        email.Subject =
            $"[ATRIUM] Consulta #{consulta.IdConsulta} - {consulta.Asunto}";

        var nombre = WebUtility.HtmlEncode(consulta.Nombre);
        var correo = WebUtility.HtmlEncode(consulta.Email);
        var tipo = WebUtility.HtmlEncode(consulta.TipoConsulta);
        var pedido = WebUtility.HtmlEncode(
            string.IsNullOrWhiteSpace(consulta.NumeroPedido)
                ? "No indicado"
                : consulta.NumeroPedido);

        var asunto = WebUtility.HtmlEncode(consulta.Asunto);

        var mensaje = WebUtility.HtmlEncode(consulta.Mensaje)
            .Replace("\r\n", "<br>")
            .Replace("\n", "<br>");

        var body = new BodyBuilder
        {
            HtmlBody = $"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="font-family:Arial,sans-serif;background:#f5f5f5;padding:30px;">
                    <div style="
                        max-width:680px;
                        margin:0 auto;
                        background:#ffffff;
                        border:1px solid #dddddd;
                        padding:32px;">

                        <h2 style="margin-top:0;">
                            Nueva consulta · ATRIUM Academy
                        </h2>

                        <p>
                            Se ha registrado una nueva consulta
                            desde la plataforma.
                        </p>

                        <hr style="border:0;border-top:1px solid #dddddd;" />

                        <p>
                            <strong>ID:</strong>
                            #{consulta.IdConsulta}
                        </p>

                        <p>
                            <strong>Nombre:</strong>
                            {nombre}
                        </p>

                        <p>
                            <strong>Correo:</strong>
                            {correo}
                        </p>

                        <p>
                            <strong>Tipo:</strong>
                            {tipo}
                        </p>

                        <p>
                            <strong>Pedido:</strong>
                            {pedido}
                        </p>

                        <p>
                            <strong>Asunto:</strong>
                            {asunto}
                        </p>

                        <p>
                            <strong>Mensaje:</strong>
                        </p>

                        <div style="
                            background:#f7f7f7;
                            border-left:3px solid #111111;
                            padding:16px;
                            line-height:1.6;">
                            {mensaje}
                        </div>

                        <hr style="
                            border:0;
                            border-top:1px solid #dddddd;
                            margin-top:28px;" />

                        <small style="color:#777777;">
                            ATRIUM Academy · Arquitectura · Diseño · Formación
                        </small>

                    </div>
                </body>
                </html>
                """
        };

        email.Body = body.ToMessageBody();

        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(
                _options.Host,
                _options.Port,
                SecureSocketOptions.StartTls,
                cancellationToken);

            /*
             * Google muestra normalmente la contraseña de aplicación
             * separada en grupos. Eliminamos espacios por seguridad.
             */
            var appPassword =
                _options.AppPassword.Replace(" ", string.Empty);

            await smtp.AuthenticateAsync(
                _options.Username,
                appPassword,
                cancellationToken);

            await smtp.SendAsync(
                email,
                cancellationToken);

            await smtp.DisconnectAsync(
                true,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo enviar la consulta {ConsultaId} por correo.",
                consulta.IdConsulta);

            throw;
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Host)
            || _options.Port <= 0
            || string.IsNullOrWhiteSpace(_options.SenderEmail)
            || string.IsNullOrWhiteSpace(_options.Username)
            || string.IsNullOrWhiteSpace(_options.AppPassword)
            || string.IsNullOrWhiteSpace(_options.RecipientEmail))
        {
            throw new InvalidOperationException(
                "La configuración SMTP de ATRIUM está incompleta.");
        }
    }
}