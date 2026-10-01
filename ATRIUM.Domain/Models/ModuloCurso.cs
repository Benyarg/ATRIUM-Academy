using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class ModuloCurso
{
    [Key]
    public int IdModulo { get; set; }

    [Required, StringLength(160)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(600)]
    public string? Descripcion { get; set; }

    [Range(0, 999)]
    public int Orden { get; set; }

    public int IdCurso { get; set; }

    [ForeignKey(nameof(IdCurso))]
    public Curso? Curso { get; set; }

    public ICollection<ContenidoModulo> Contenidos { get; set; } = new List<ContenidoModulo>();
}
