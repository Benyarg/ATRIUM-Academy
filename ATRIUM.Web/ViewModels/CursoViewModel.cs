using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Web.ViewModels;

public class CursoViewModel : IValidatableObject
{
    public int IdCurso { get; set; }

    [Required, StringLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1200)]
    public string? Descripcion { get; set; }

    [Range(0, 999999)]
    public decimal Precio { get; set; }

    [Range(0, 999999)]
    public decimal PrecioDescuento { get; set; }

    [StringLength(160)]
    public string? DocenteAsignado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    public int IdCategoria { get; set; }

    public IFormFile? ImagenArchivo { get; set; }
    public string? ImagenActual { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PrecioDescuento > 0 && PrecioDescuento >= Precio)
        {
            yield return new ValidationResult(
                "El precio con descuento debe ser menor que el precio regular.",
                [nameof(PrecioDescuento)]);
        }
    }
}
