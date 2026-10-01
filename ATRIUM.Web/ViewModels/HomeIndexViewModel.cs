using ATRIUM.Domain.Models;

namespace ATRIUM.Web.ViewModels;

public sealed class HomeIndexViewModel
{
    public IReadOnlyList<Curso> CursosDestacados { get; init; } = Array.Empty<Curso>();
    public IReadOnlyList<Carousel> Banners { get; init; } = Array.Empty<Carousel>();
    public IReadOnlyList<HomeCategoriaViewModel> Categorias { get; init; } = Array.Empty<HomeCategoriaViewModel>();
    public int TotalCursos { get; init; }
    public int TotalRecursos { get; init; }
}

public sealed class HomeCategoriaViewModel
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Descripcion { get; init; } = string.Empty;
    public int TotalCursos { get; init; }
}
