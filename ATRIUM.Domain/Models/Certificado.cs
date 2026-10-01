using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class Certificado
{
    [Key]
    public int IdCertificado { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string RutaArchivo { get; set; } = string.Empty;

    [StringLength(50)]
    public string CodigoUnico { get; set; } = string.Empty;

    public int IdCurso { get; set; }

    [ForeignKey(nameof(IdCurso))]
    public Curso? Curso { get; set; }

    [Required]
    public string IdUsuario { get; set; } = string.Empty;

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }
}
