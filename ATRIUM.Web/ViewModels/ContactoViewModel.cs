using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels;

public class ContactoViewModel
{
    [Required(ErrorMessage = "Ingresa tu nombre.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingresa tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "Selecciona el tipo de consulta.")]
    [StringLength(50)]
    public string TipoConsulta { get; set; } = string.Empty;


    [StringLength(50)]
    public string? NumeroPedido { get; set; }


    [Required(ErrorMessage = "Ingresa el asunto.")]
    [StringLength(
        150,
        MinimumLength = 5,
        ErrorMessage = "El asunto debe contener entre 5 y 150 caracteres.")]
    public string Asunto { get; set; } = string.Empty;


    [Required(ErrorMessage = "Describe tu consulta.")]
    [StringLength(
        2000,
        MinimumLength = 10,
        ErrorMessage = "El mensaje debe contener entre 10 y 2000 caracteres.")]
    public string Mensaje { get; set; } = string.Empty;
}