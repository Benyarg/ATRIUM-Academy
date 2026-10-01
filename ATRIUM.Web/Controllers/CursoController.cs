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

namespace ATRIUM.Web.Controllers;

public class CursoController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly ICourseAccessService _courseAccess;
    private readonly IImageStorageService _imageStorage;

    public CursoController(
        AtriumDbContext context,
        UserManager<Usuario> userManager,
        ICourseAccessService courseAccess,
        IImageStorageService imageStorage)
    {
        _context = context;
        _userManager = userManager;
        _courseAccess = courseAccess;
        _imageStorage = imageStorage;
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Index() =>
        View(await _context.Cursos
            .AsNoTracking()
            .Include(course => course.Categoria)
            .OrderByDescending(course => course.IdCurso)
            .ToListAsync());

    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var course = await _context.Cursos
            .AsNoTracking()
            .Include(item => item.Categoria)
            .Include(item => item.Modulos)
                .ThenInclude(module => module.Contenidos)
            .FirstOrDefaultAsync(item => item.IdCurso == id);

        if (course is null)
            return NotFound();

        var hasAccess = User.IsInRole(AppRoles.Administrator);
        if (!hasAccess && User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrWhiteSpace(userId))
                hasAccess = await _courseAccess.HasApprovedAccessAsync(userId, course.IdCurso);
        }

        ViewBag.TieneAcceso = hasAccess;
        return View(course);
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View(new CursoViewModel());
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CursoViewModel model)
    {
        await ValidateCategoryAsync(model.IdCategoria);

        var imageError = await _imageStorage.ValidateAsync(
            model.ImagenArchivo,
            required: false,
            HttpContext.RequestAborted);

        if (imageError is not null)
            ModelState.AddModelError(nameof(model.ImagenArchivo), imageError);

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(model.IdCategoria);
            return View(model);
        }

        var imagePath = model.ImagenArchivo is null
            ? null
            : await _imageStorage.SaveAsync(
                model.ImagenArchivo,
                ImageStorageArea.Courses,
                HttpContext.RequestAborted);

        var course = new Curso
        {
            Nombre = model.Nombre.Trim(),
            Descripcion = model.Descripcion?.Trim(),
            Precio = model.Precio,
            PrecioDescuento = model.PrecioDescuento,
            DocenteAsignado = model.DocenteAsignado?.Trim(),
            IdCategoria = model.IdCategoria,
            URLImagen = imagePath
        };

        _context.Cursos.Add(course);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            _imageStorage.Delete(imagePath, ImageStorageArea.Courses);
            throw;
        }

        TempData["SuccessMessage"] = "Curso creado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var course = await _context.Cursos.FindAsync(id);
        if (course is null)
            return NotFound();

        await LoadCategoriesAsync(course.IdCategoria);
        return View(ToViewModel(course));
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CursoViewModel model)
    {
        if (id != model.IdCurso)
            return NotFound();

        var course = await _context.Cursos.FindAsync(id);
        if (course is null)
            return NotFound();

        await ValidateCategoryAsync(model.IdCategoria);

        var imageError = await _imageStorage.ValidateAsync(
            model.ImagenArchivo,
            required: false,
            HttpContext.RequestAborted);

        if (imageError is not null)
            ModelState.AddModelError(nameof(model.ImagenArchivo), imageError);

        if (!ModelState.IsValid)
        {
            model.ImagenActual = course.URLImagen;
            await LoadCategoriesAsync(model.IdCategoria);
            return View(model);
        }

        course.Nombre = model.Nombre.Trim();
        course.Descripcion = model.Descripcion?.Trim();
        course.Precio = model.Precio;
        course.PrecioDescuento = model.PrecioDescuento;
        course.DocenteAsignado = model.DocenteAsignado?.Trim();
        course.IdCategoria = model.IdCategoria;

        var previousImagePath = course.URLImagen;
        string? newImagePath = null;

        if (model.ImagenArchivo is not null)
        {
            newImagePath = await _imageStorage.SaveAsync(
                model.ImagenArchivo,
                ImageStorageArea.Courses,
                HttpContext.RequestAborted);

            course.URLImagen = newImagePath;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            _imageStorage.Delete(newImagePath, ImageStorageArea.Courses);
            throw;
        }

        if (newImagePath is not null)
            _imageStorage.Delete(previousImagePath, ImageStorageArea.Courses);

        TempData["SuccessMessage"] = "Curso actualizado.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var course = await _context.Cursos
            .AsNoTracking()
            .Include(item => item.Categoria)
            .FirstOrDefaultAsync(item => item.IdCurso == id);

        return course is null ? NotFound() : View(course);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _context.Cursos
            .Where(course => course.IdCurso == id)
            .Select(course => new
            {
                Course = course,
                HasDependencies = course.Modulos.Any()
                                  || course.PedidoDetalles.Any()
                                  || course.CarritoCompras.Any()
                                  || course.ContenidosEducativos.Any()
                                  || course.Certificados.Any()
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return RedirectToAction(nameof(Index));

        if (result.HasDependencies)
        {
            TempData["ErrorMessage"] = "No puedes eliminar un curso que todavía tiene módulos, compras, recursos o certificados asociados.";
            return RedirectToAction(nameof(Index));
        }

        _context.Cursos.Remove(result.Course);
        await _context.SaveChangesAsync();
        _imageStorage.Delete(result.Course.URLImagen, ImageStorageArea.Courses);

        TempData["SuccessMessage"] = "Curso eliminado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator)]
    public async Task<IActionResult> MisCursos()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var courses = await _context.Cursos
            .AsNoTracking()
            .Include(course => course.Categoria)
            .Where(course => course.PedidoDetalles.Any(detail =>
                detail.Pedido != null
                && detail.Pedido.UserId == userId
                && detail.Pedido.EstadoPedido == OrderStatuses.Approved))
            .OrderBy(course => course.Nombre)
            .ToListAsync();

        return View(courses);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Buscar(string? query)
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
                || (course.Descripcion ?? string.Empty).Contains(term));
        }

        return View(await courses.OrderBy(course => course.Nombre).ToListAsync());
    }

    private async Task LoadCategoriesAsync(int? selectedId = null)
    {
        var categories = await _context.Categorias
            .AsNoTracking()
            .Where(category => category.Estado)
            .OrderBy(category => category.Nombre)
            .Select(category => new { category.Id, category.Nombre })
            .ToListAsync();

        ViewData["IdCategoria"] = new SelectList(categories, "Id", "Nombre", selectedId);
    }

    private async Task ValidateCategoryAsync(int categoryId)
    {
        if (categoryId <= 0
            || !await _context.Categorias.AsNoTracking().AnyAsync(category => category.Id == categoryId && category.Estado))
        {
            ModelState.AddModelError(nameof(CursoViewModel.IdCategoria), "Selecciona una categoría activa y válida.");
        }
    }

    private static CursoViewModel ToViewModel(Curso course) => new()
    {
        IdCurso = course.IdCurso,
        Nombre = course.Nombre,
        Descripcion = course.Descripcion,
        Precio = course.Precio,
        PrecioDescuento = course.PrecioDescuento,
        DocenteAsignado = course.DocenteAsignado,
        IdCategoria = course.IdCategoria,
        ImagenActual = course.URLImagen
    };
}
