using ATRIUM.Domain.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class Pedido
{
    [Key]
    public int IdPedido { get; set; }

    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

    [Required, StringLength(40)]
    public string FormaPago { get; set; } = PaymentMethods.Yape;

    [Required, StringLength(30)]
    public string EstadoPedido { get; set; } = OrderStatuses.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPedido { get; set; }

    [Required, StringLength(220)]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(20)]
    public string CodigoPostal { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Provincia { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Localidad { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public Usuario? Usuario { get; set; }

    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
}
