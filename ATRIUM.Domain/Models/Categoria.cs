using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ATRIUM.Domain.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Descripcion { get; set; } = string.Empty;

    public bool Estado { get; set; } = true;

    [JsonIgnore]
    public ICollection<Curso> Cursos { get; set; } = new List<Curso>();
}
