using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.Services;
using ATRIUM.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ATRIUM.Web.Controllers;

public class TiendaController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly ICourseAccessService _courseAccess;

    public TiendaController(
        AtriumDbContext context,
        UserManager<Usuario> userManager,
        ICourseAccessService courseAccess)
    {
        _context = context;
        _userManager = userManager;
        _courseAccess = courseAccess;
    }

    [AllowAnonymous]
    public async Task<IActionResult> CursosDisponibles(string? query, int? categoria)
    {
        var courses = _context.Cursos
            .AsNoTracking()
            .Include(course => course.Categoria)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            courses = courses.Where(course =>
                course.Nombre.Contains(term)
                || (course.Descripcion ?? string.Empty).Contains(term)
                || (course.Categoria != null && course.Categoria.Nombre.Contains(term)));
        }

        if (categoria.HasValue)
            courses = courses.Where(course => course.IdCategoria == categoria.Value);

        var categories = await _context.Categorias
            .AsNoTracking()
            .Where(category => category.Estado)
            .OrderBy(category => category.Nombre)
            .Select(category => new { category.Id, category.Nombre })
            .ToListAsync();

        ViewBag.Categorias = new SelectList(categories, "Id", "Nombre", categoria);
        ViewBag.Query = query;

        return View(await courses.OrderBy(course => course.Nombre).ToListAsync());
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator), HttpGet]
    public async Task<IActionResult> FinalizarPedido(int idCurso)
    {
        var course = await _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.IdCurso == idCurso);

        if (course is null)
            return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var redirect = await RedirectIfCourseUnavailableAsync(user.Id, idCurso);
        if (redirect is not null)
            return redirect;

        return View(new PedidoViewModel
        {
            IdCurso = course.IdCurso,
            NombreCurso = course.Nombre,
            TotalPedido = course.PrecioFinal,
            NombreUsuario = $"{user.Nombre} {user.Apellido}",
            Email = user.Email ?? string.Empty,
            Direccion = user.Direccion,
            Telefono = user.Telefono,
            Provincia = "Cajamarca",
            Localidad = "Cajamarca",
            FormaPago = PaymentMethods.Yape
        });
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarPedido(PedidoViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        var course = await _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.IdCurso == model.IdCurso);

        if (course is null)
            return NotFound();

        model.NombreCurso = course.Nombre;
        model.TotalPedido = course.PrecioFinal;
        model.NombreUsuario = $"{user.Nombre} {user.Apellido}";
        model.Email = user.Email ?? string.Empty;

        if (!PaymentMethods.All.Contains(model.FormaPago, StringComparer.Ordinal))
            ModelState.AddModelError(nameof(model.FormaPago), "Selecciona un método de pago válido.");

        if (!ModelState.IsValid)
            return View("FinalizarPedido", model);

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);

        var redirect = await RedirectIfCourseUnavailableAsync(user.Id, course.IdCurso);
        if (redirect is not null)
            return redirect;

        var order = new Pedido
        {
            FechaPedido = DateTime.UtcNow,
            FormaPago = model.FormaPago,
            EstadoPedido = OrderStatuses.Pending,
            TotalPedido = course.PrecioFinal,
            Direccion = model.Direccion.Trim(),
            CodigoPostal = model.CodigoPostal?.Trim() ?? string.Empty,
            Provincia = model.Provincia.Trim(),
            Localidad = model.Localidad.Trim(),
            Telefono = model.Telefono.Trim(),
            UserId = user.Id,
            Detalles =
            [
                new PedidoDetalle
                {
                    IdCurso = course.IdCurso,
                    Cantidad = 1,
                    PrecioIndividual = course.PrecioFinal
                }
            ]
        };

        _context.Pedidos.Add(order);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);

        return RedirectToAction(nameof(ConfirmacionPago), new { id = order.IdPedido });
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator)]
    public async Task<IActionResult> ConfirmacionPago(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var order = await FindUserOrderAsync(id, userId);
        return order is null ? NotFound() : View(order);
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator)]
    public async Task<IActionResult> PagoYape(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var order = await FindUserOrderAsync(id, userId);
        return order is null ? NotFound() : View(order);
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator)]
    public async Task<IActionResult> Pagar(int id)
    {
        var course = await _context.Cursos
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.IdCurso == id);

        if (course is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var redirect = await RedirectIfCourseUnavailableAsync(userId, id);
        return redirect ?? View("PagarCurso", course);
    }

    private async Task<Pedido?> FindUserOrderAsync(int orderId, string userId) =>
        await _context.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.IdPedido == orderId && order.UserId == userId);

    private async Task<IActionResult?> RedirectIfCourseUnavailableAsync(string userId, int courseId)
    {
        var blockingStatus = await _courseAccess.GetBlockingOrderStatusAsync(userId, courseId);

        if (blockingStatus == OrderStatuses.Approved)
        {
            TempData["ErrorMessage"] = "Ya tienes acceso a este curso.";
            return RedirectToAction("MisCursos", "Curso");
        }

        if (blockingStatus == OrderStatuses.Pending)
        {
            TempData["ErrorMessage"] = "Ya existe un pedido pendiente para este curso.";
            return RedirectToAction("MisPedidos", "Pedido");
        }

        return null;
    }
}
