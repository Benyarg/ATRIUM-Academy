using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATRIUM.Domain.Models;

public class Curso
{
    [Key]
    public int IdCurso { get; set; }

    [Required, StringLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1200)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "decimal(18,2)"), Range(0, 999999)]
    public decimal Precio { get; set; }

    [Column(TypeName = "decimal(18,2)"), Range(0, 999999)]
    public decimal PrecioDescuento { get; set; }

    [StringLength(500)]
    public string? URLImagen { get; set; }

    [StringLength(160)]
    public string? DocenteAsignado { get; set; }

    public int IdCategoria { get; set; }

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    public ICollection<ModuloCurso> Modulos { get; set; } = new List<ModuloCurso>();
    public ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
    public ICollection<CarroCompras> CarritoCompras { get; set; } = new List<CarroCompras>();
    public ICollection<ContenidoEducativo> ContenidosEducativos { get; set; } = new List<ContenidoEducativo>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();

    [NotMapped]
    public decimal PrecioFinal => PrecioDescuento > 0 && PrecioDescuento < Precio ? PrecioDescuento : Precio;
}
