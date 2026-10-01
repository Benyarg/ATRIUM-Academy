using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.StudentOrAdministrator)]
public class CarroComprasController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly ICourseAccessService _courseAccess;

    public CarroComprasController(
        AtriumDbContext context,
        UserManager<Usuario> userManager,
        ICourseAccessService courseAccess)
    {
        _context = context;
        _userManager = userManager;
        _courseAccess = courseAccess;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var items = await _context.CarritoCompras
            .AsNoTracking()
            .Where(item => item.UsuarioId == userId)
            .Include(item => item.Curso)
                .ThenInclude(course => course!.Categoria)
            .OrderByDescending(item => item.IdCarroCompras)
            .ToListAsync();

        return View(items);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int idCurso)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!await _context.Cursos.AsNoTracking().AnyAsync(course => course.IdCurso == idCurso))
            return NotFound();

        var blockingStatus = await _courseAccess.GetBlockingOrderStatusAsync(userId, idCurso);
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

        if (await _context.CarritoCompras.AnyAsync(item => item.UsuarioId == userId && item.IdCurso == idCurso))
        {
            TempData["ErrorMessage"] = "Ese curso ya está en tu carrito.";
            return RedirectToAction(nameof(Index));
        }

        _context.CarritoCompras.Add(new CarroCompras
        {
            UsuarioId = userId,
            IdCurso = idCurso,
            Cantidad = 1
        });

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Curso agregado al carrito.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "No se pudo agregar el curso. Actualiza el carrito e inténtalo nuevamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var item = await _context.CarritoCompras
            .FirstOrDefaultAsync(cartItem => cartItem.IdCarroCompras == id && cartItem.UsuarioId == userId);

        if (item is not null)
        {
            _context.CarritoCompras.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Comprar()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);

        var cart = await _context.CarritoCompras
            .Where(item => item.UsuarioId == user.Id)
            .Include(item => item.Curso)
            .ToListAsync(HttpContext.RequestAborted);

        if (cart.Count == 0)
        {
            TempData["ErrorMessage"] = "Tu carrito está vacío.";
            return RedirectToAction(nameof(Index));
        }

        var unavailableCourseIds = await _courseAccess.GetUnavailableCourseIdsAsync(user.Id, HttpContext.RequestAborted);
        var purchasableItems = cart
            .Where(item => item.Curso is not null && !unavailableCourseIds.Contains(item.IdCurso))
            .ToList();

        if (purchasableItems.Count == 0)
        {
            _context.CarritoCompras.RemoveRange(cart);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
            await transaction.CommitAsync(HttpContext.RequestAborted);

            TempData["ErrorMessage"] = "Los cursos del carrito ya tienen acceso aprobado o un pedido pendiente.";
            return RedirectToAction("MisPedidos", "Pedido");
        }

        var order = new Pedido
        {
            UserId = user.Id,
            FechaPedido = DateTime.UtcNow,
            EstadoPedido = OrderStatuses.Pending,
            FormaPago = PaymentMethods.Yape,
            Direccion = user.Direccion,
            CodigoPostal = string.Empty,
            Provincia = "Cajamarca",
            Localidad = "Cajamarca",
            Telefono = user.Telefono,
            TotalPedido = purchasableItems.Sum(item => item.Curso!.PrecioFinal),
            Detalles = purchasableItems
                .Select(item => new PedidoDetalle
                {
                    IdCurso = item.IdCurso,
                    Cantidad = 1,
                    PrecioIndividual = item.Curso!.PrecioFinal
                })
                .ToList()
        };

        _context.Pedidos.Add(order);
        _context.CarritoCompras.RemoveRange(cart);
        await _context.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);

        if (purchasableItems.Count != cart.Count)
            TempData["InfoMessage"] = "Se omitieron cursos que ya tenían acceso o un pedido pendiente.";

        return RedirectToAction("ConfirmacionPago", "Tienda", new { id = order.IdPedido });
    }
}
