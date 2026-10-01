using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class ProgresoEstudiante
{
    [Key]
    public int IdProgreso { get; set; }

    [Required]
    public string IdUsuario { get; set; } = string.Empty;

    [ForeignKey(nameof(IdUsuario))]
    public Usuario? Usuario { get; set; }

    public int IdContenido { get; set; }

    [ForeignKey(nameof(IdContenido))]
    public ContenidoModulo? ContenidoModulo { get; set; }

    public bool Completado { get; set; }
    public DateTime FechaCompletado { get; set; } = DateTime.MinValue;
}
