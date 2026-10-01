using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class ContenidoModulo
{
    [Key]
    public int IdContenido { get; set; }

    [Required, StringLength(160)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(40)]
    public string Tipo { get; set; } = ModuleContentTypes.Video;

    [Required, StringLength(500), HttpUrl]
    public string URLContenido { get; set; } = string.Empty;

    [Range(0, 999)]
    public int Orden { get; set; }

    public int IdModulo { get; set; }

    [ForeignKey(nameof(IdModulo))]
    public ModuloCurso? ModuloCurso { get; set; }

    public ICollection<ProgresoEstudiante> Progresos { get; set; } = new List<ProgresoEstudiante>();
}
