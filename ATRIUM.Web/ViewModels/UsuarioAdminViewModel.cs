using ATRIUM.Domain.Constants;
using ATRIUM.Web.ViewModels.Validation;
using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels;

public class UsuarioAdminViewModel : IValidatableObject
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [DataType(DataType.Date), ValidBirthDate]
    public DateTime FechaDeNacimiento { get; set; }

    public string Rol { get; set; } = AppRoles.Student;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!AppRoles.All.Contains(Rol, StringComparer.Ordinal))
        {
            yield return new ValidationResult(
                "Selecciona un rol válido.",
                [nameof(Rol)]);
        }
    }
}


public sealed class UsuarioListaItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}
