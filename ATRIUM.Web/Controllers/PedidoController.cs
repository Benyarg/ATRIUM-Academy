using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.StudentOrAdministrator)]
public class PedidoController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly UserManager<Usuario> _userManager;

    public PedidoController(AtriumDbContext context, UserManager<Usuario> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Index(string? estado)
    {
        var orders = _context.Pedidos
            .AsNoTracking()
            .Include(order => order.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado) && OrderStatuses.All.Contains(estado, StringComparer.Ordinal))
            orders = orders.Where(order => order.EstadoPedido == estado);

        return View(await orders.OrderByDescending(order => order.FechaPedido).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var order = await _context.Pedidos
            .AsNoTracking()
            .Include(item => item.Detalles)
                .ThenInclude(detail => detail.Curso)
            .FirstOrDefaultAsync(item => item.IdPedido == id);

        if (order is null)
            return NotFound();

        var userId = _userManager.GetUserId(User);
        if (!User.IsInRole(AppRoles.Administrator) && order.UserId != userId)
            return Forbid();

        return View(order);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
    {
        if (!OrderStatuses.All.Contains(nuevoEstado, StringComparer.Ordinal))
            return BadRequest();

        var order = await _context.Pedidos.FindAsync(id);
        if (order is null)
            return NotFound();

        order.EstadoPedido = nuevoEstado;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Pedido #{id} actualizado a {nuevoEstado}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> MisPedidos()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var orders = await _context.Pedidos
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.FechaPedido)
            .ToListAsync();

        return View(orders);
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.IdPedido == id);

        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Pedidos
            .Include(order => order.Detalles)
            .FirstOrDefaultAsync(order => order.IdPedido == id);

        if (item is null)
            return RedirectToAction(nameof(Index));

        if (item.EstadoPedido == OrderStatuses.Approved)
        {
            TempData["ErrorMessage"] = "No se puede eliminar un pedido aprobado porque forma parte del historial de acceso y ventas.";
            return RedirectToAction(nameof(Index));
        }

        _context.PedidoDetalles.RemoveRange(item.Detalles);
        _context.Pedidos.Remove(item);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Pedido eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
