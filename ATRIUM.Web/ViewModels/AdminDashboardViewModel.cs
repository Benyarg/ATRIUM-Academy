namespace ATRIUM.Web.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsuarios { get; set; }
    public int TotalCursos { get; set; }
    public int PedidosPendientes { get; set; }
    public int PedidosAprobados { get; set; }
    public decimal VentasAprobadas { get; set; }
    public IReadOnlyList<PedidoResumenViewModel> UltimosPedidos { get; set; } = Array.Empty<PedidoResumenViewModel>();
}

public class PedidoResumenViewModel
{
    public int IdPedido { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
}
