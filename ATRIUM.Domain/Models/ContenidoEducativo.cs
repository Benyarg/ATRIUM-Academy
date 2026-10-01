using ATRIUM.Domain.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class ContenidoEducativo
{
    [Key]
    public int ContenidoId { get; set; }

    [Required, StringLength(150)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    [StringLength(500), HttpUrl]
    public string? UrlContenido { get; set; }

    [Required]
    public int CursoId { get; set; }

    [ForeignKey(nameof(CursoId))]
    public Curso? Curso { get; set; }

    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public bool EsActivo { get; set; } = true;
}
