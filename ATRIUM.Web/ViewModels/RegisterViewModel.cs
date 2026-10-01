using ATRIUM.Web.ViewModels.Validation;
using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio."), StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio."), StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio."), EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria."), StringLength(220)]
    public string Direccion { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Ingresa un teléfono válido."), StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria."), DataType(DataType.Date), ValidBirthDate]
    public DateTime FechaDeNacimiento { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria."), DataType(DataType.Password), MinLength(8, ErrorMessage = "Usa al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contraseña."), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
