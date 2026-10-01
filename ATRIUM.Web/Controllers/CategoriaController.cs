using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class CategoriaController : Controller
{
    private readonly AtriumDbContext _context;

    public CategoriaController(AtriumDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index() =>
        View(await _context.Categorias
            .AsNoTracking()
            .OrderBy(category => category.Nombre)
            .ToListAsync());

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Categorias
            .AsNoTracking()
            .Include(category => category.Cursos)
            .FirstOrDefaultAsync(category => category.Id == id);

        return item is null ? NotFound() : View(item);
    }

    public IActionResult Create() => View(new Categoria { Estado = true });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        categoria.Nombre = categoria.Nombre?.Trim() ?? string.Empty;
        categoria.Descripcion = categoria.Descripcion?.Trim() ?? string.Empty;

        await ValidateUniqueNameAsync(categoria.Nombre, excludedId: null);
        if (!ModelState.IsValid)
            return View(categoria);

        var newCategory = new Categoria
        {
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado
        };

        _context.Categorias.Add(newCategory);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Categoría creada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Categorias.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria posted)
    {
        if (id != posted.Id)
            return NotFound();

        var current = await _context.Categorias.FindAsync(id);
        if (current is null)
            return NotFound();

        posted.Nombre = posted.Nombre?.Trim() ?? string.Empty;
        posted.Descripcion = posted.Descripcion?.Trim() ?? string.Empty;

        await ValidateUniqueNameAsync(posted.Nombre, id);
        if (!ModelState.IsValid)
            return View(posted);

        current.Nombre = posted.Nombre;
        current.Descripcion = posted.Descripcion;
        current.Estado = posted.Estado;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Categoría actualizada.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);

        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _context.Categorias
            .Where(category => category.Id == id)
            .Select(category => new
            {
                Category = category,
                HasCourses = category.Cursos.Any()
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return RedirectToAction(nameof(Index));

        if (result.HasCourses)
        {
            TempData["ErrorMessage"] = "No puedes eliminar una categoría que todavía contiene cursos.";
            return RedirectToAction(nameof(Index));
        }

        _context.Categorias.Remove(result.Category);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Categoría eliminada.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueNameAsync(string name, int? excludedId)
    {
        var exists = await _context.Categorias
            .AsNoTracking()
            .AnyAsync(category => category.Nombre == name
                                  && (!excludedId.HasValue || category.Id != excludedId.Value));

        if (exists)
            ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre.");
    }
}
