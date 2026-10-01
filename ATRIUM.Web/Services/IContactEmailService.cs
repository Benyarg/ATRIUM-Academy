using ATRIUM.Domain.Models;

namespace ATRIUM.Web.Services;

public interface IContactEmailService
{
    Task SendContactNotificationAsync(
        ConsultaContacto consulta,
        CancellationToken cancellationToken = default);
}