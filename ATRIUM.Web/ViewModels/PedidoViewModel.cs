using ATRIUM.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels;

public class PedidoViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un curso válido.")]
    public int IdCurso { get; set; }

    public string NombreCurso { get; set; } = string.Empty;
    public decimal TotalPedido { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria."), StringLength(220)]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(20)]
    public string CodigoPostal { get; set; } = string.Empty;

    [Required(ErrorMessage = "La provincia es obligatoria."), StringLength(100)]
    public string Provincia { get; set; } = string.Empty;

    [Required(ErrorMessage = "La localidad es obligatoria."), StringLength(100)]
    public string Localidad { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio."), Phone, StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un método de pago.")]
    public string FormaPago { get; set; } = PaymentMethods.Yape;
}
