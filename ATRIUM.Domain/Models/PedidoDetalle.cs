using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class PedidoDetalle
{
    [Key]
    public int IdDetallePedido { get; set; }

    [Range(1, 10)]
    public int Cantidad { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioIndividual { get; set; }

    public int IdPedido { get; set; }

    [ForeignKey(nameof(IdPedido))]
    public Pedido? Pedido { get; set; }

    public int IdCurso { get; set; }

    [ForeignKey(nameof(IdCurso))]
    public Curso? Curso { get; set; }
}
