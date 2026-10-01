using System.ComponentModel.DataAnnotations;

namespace ATRIUM.Domain.Models;

public class Carousel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Descripcion { get; set; } = string.Empty;

    [Display(Name = "Imagen")]
    public string ImagenUrl { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
    public int Orden { get; set; }
}
