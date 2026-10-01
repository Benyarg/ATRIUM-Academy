using ATRIUM.Web.ViewModels.Validation;
using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels;

public class PerfilViewModel
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Direccion { get; set; } = string.Empty;

    [Phone, StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [DataType(DataType.Date), ValidBirthDate]
    public DateTime FechaDeNacimiento { get; set; }
}
