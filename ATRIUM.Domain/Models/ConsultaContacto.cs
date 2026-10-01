using ATRIUM.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class ConsultaContacto
{
    [Key]
    public int IdConsulta { get; set; }

    [StringLength(450)]
    public string? UsuarioId { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public Usuario? Usuario { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string TipoConsulta { get; set; } = string.Empty;

    [StringLength(50)]
    public string? NumeroPedido { get; set; }

    [Required]
    [StringLength(150)]
    public string Asunto { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Mensaje { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = ContactStatuses.Pending;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public bool CorreoNotificacionEnviado { get; set; }

    public DateTime? FechaCorreoEnviado { get; set; }
}