using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class AdminController : Controller
{
    private readonly UserManager<Usuario> _userManager;
    private readonly AtriumDbContext _context;

    public AdminController(UserManager<Usuario> userManager, AtriumDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var recentOrders = await _context.Pedidos
            .AsNoTracking()
            .OrderByDescending(order => order.FechaPedido)
            .Take(6)
            .Select(order => new PedidoResumenViewModel
            {
                IdPedido = order.IdPedido,
                Usuario = order.Usuario != null
                    ? order.Usuario.Nombre + " " + order.Usuario.Apellido
                    : "Usuario",
                Fecha = order.FechaPedido,
                Total = order.TotalPedido,
                Estado = order.EstadoPedido
            })
            .ToListAsync();

        var orderStats = await _context.Pedidos
            .AsNoTracking()
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Pending = group.Count(order => order.EstadoPedido == OrderStatuses.Pending),
                Approved = group.Count(order => order.EstadoPedido == OrderStatuses.Approved),
                ApprovedSales = group
                    .Where(order => order.EstadoPedido == OrderStatuses.Approved)
                    .Sum(order => (decimal?)order.TotalPedido) ?? 0
            })
            .FirstOrDefaultAsync();

        var model = new AdminDashboardViewModel
        {
            TotalUsuarios = await _userManager.Users.CountAsync(),
            TotalCursos = await _context.Cursos.CountAsync(),
            PedidosPendientes = orderStats?.Pending ?? 0,
            PedidosAprobados = orderStats?.Approved ?? 0,
            VentasAprobadas = orderStats?.ApprovedSales ?? 0,
            UltimosPedidos = recentOrders
        };

        return View(model);
    }

    public async Task<IActionResult> ListaUsuarios(string? query)
    {
        var users = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            users = users.Where(user =>
                (user.Email ?? string.Empty).Contains(term)
                || user.Nombre.Contains(term)
                || user.Apellido.Contains(term));
        }

        return View(await users
            .OrderBy(user => user.Nombre)
            .ThenBy(user => user.Apellido)
            .Select(user => new UsuarioListaItemViewModel
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email ?? string.Empty,
                Telefono = user.Telefono
            })
            .ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> EditarUsuario(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return View(ToAdminViewModel(user, roles));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarUsuario(UsuarioAdminViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
            return NotFound();

        var email = model.Email.Trim();
        var emailOwner = await _userManager.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "Ese correo pertenece a otra cuenta.");
            return View(model);
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var isCurrentlyAdministrator = currentRoles.Contains(AppRoles.Administrator, StringComparer.Ordinal);
        var currentAdmin = await _userManager.GetUserAsync(User);

        if (currentAdmin?.Id == user.Id && model.Rol != AppRoles.Administrator)
        {
            ModelState.AddModelError(nameof(model.Rol), "No puedes quitarte tu propio rol de administrador durante una sesión administrativa.");
            return View(model);
        }

        if (isCurrentlyAdministrator
            && model.Rol != AppRoles.Administrator
            && await CountAdministratorsAsync() <= 1)
        {
            ModelState.AddModelError(nameof(model.Rol), "Debe existir al menos un administrador en el sistema.");
            return View(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        user.Nombre = model.Nombre.Trim();
        user.Apellido = model.Apellido.Trim();
        user.Email = email;
        user.UserName = email;
        user.Direccion = model.Direccion.Trim();
        user.Telefono = model.Telefono.Trim();
        user.FechaDeNacimiento = model.FechaDeNacimiento.Date;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            await transaction.RollbackAsync();
            AddIdentityErrors(updateResult);
            return View(model);
        }

        if (!currentRoles.Contains(model.Rol, StringComparer.Ordinal))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Rol);
            if (!addRoleResult.Succeeded)
            {
                await transaction.RollbackAsync();
                AddIdentityErrors(addRoleResult);
                return View(model);
            }
        }

        var rolesToRemove = currentRoles
            .Where(role => AppRoles.All.Contains(role, StringComparer.Ordinal)
                           && !string.Equals(role, model.Rol, StringComparison.Ordinal))
            .ToArray();

        if (rolesToRemove.Length > 0)
        {
            var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeRolesResult.Succeeded)
            {
                await transaction.RollbackAsync();
                AddIdentityErrors(removeRolesResult);
                return View(model);
            }
        }

        // Invalida las cookies existentes del usuario cuando un administrador cambia
        // sus datos o permisos. Así, una degradación de rol no queda activa hasta
        // que expire la sesión anterior.
        var securityStampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!securityStampResult.Succeeded)
        {
            await transaction.RollbackAsync();
            AddIdentityErrors(securityStampResult);
            return View(model);
        }

        await transaction.CommitAsync();
        TempData["SuccessMessage"] = "Usuario actualizado.";

        return RedirectToAction(nameof(ListaUsuarios));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == id)
        {
            TempData["ErrorMessage"] = "No puedes eliminar tu propia cuenta de administración.";
            return RedirectToAction(nameof(ListaUsuarios));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
            return NotFound();

        if (await _userManager.IsInRoleAsync(user, AppRoles.Administrator))
        {
            TempData["ErrorMessage"] = "Cambia primero el rol del administrador antes de eliminarlo.";
            return RedirectToAction(nameof(ListaUsuarios));
        }

        if (await HasBusinessHistoryAsync(id))
        {
            TempData["ErrorMessage"] = "No se puede eliminar una cuenta con pedidos, progreso o certificados asociados. Conserva la cuenta para mantener la trazabilidad.";
            return RedirectToAction(nameof(ListaUsuarios));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await RemoveDeletableDependenciesAsync(id);
        var deleteResult = await _userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "No se pudo eliminar el usuario.";
            return RedirectToAction(nameof(ListaUsuarios));
        }

        await transaction.CommitAsync();
        TempData["SuccessMessage"] = "Usuario eliminado.";

        return RedirectToAction(nameof(ListaUsuarios));
    }

    private Task<int> CountAdministratorsAsync() =>
        _context.UserRoles.CountAsync(userRole =>
            _context.Roles.Any(role =>
                role.Id == userRole.RoleId
                && role.Name == AppRoles.Administrator));

    private async Task<bool> HasBusinessHistoryAsync(string userId) =>
        await _context.Pedidos.AnyAsync(order => order.UserId == userId)
        || await _context.ProgresosEstudiante.AnyAsync(progress => progress.IdUsuario == userId)
        || await _context.Certificados.AnyAsync(certificate => certificate.IdUsuario == userId);

    private async Task RemoveDeletableDependenciesAsync(string userId)
    {
        _context.CarritoCompras.RemoveRange(_context.CarritoCompras.Where(item => item.UsuarioId == userId));
        _context.UserRoles.RemoveRange(_context.UserRoles.Where(item => item.UserId == userId));
        _context.UserClaims.RemoveRange(_context.UserClaims.Where(item => item.UserId == userId));
        _context.UserLogins.RemoveRange(_context.UserLogins.Where(item => item.UserId == userId));
        _context.UserTokens.RemoveRange(_context.UserTokens.Where(item => item.UserId == userId));
        await _context.SaveChangesAsync();
    }

    private static UsuarioAdminViewModel ToAdminViewModel(
        Usuario user,
        IList<string> roles) => new()
    {
        Id = user.Id,
        Nombre = user.Nombre,
        Apellido = user.Apellido,
        Email = user.Email ?? string.Empty,
        Direccion = user.Direccion,
        Telefono = user.Telefono,
        FechaDeNacimiento = user.FechaDeNacimiento,
        Rol = roles.Contains(AppRoles.Administrator, StringComparer.Ordinal)
            ? AppRoles.Administrator
            : AppRoles.Student
    };

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);
    }
}
