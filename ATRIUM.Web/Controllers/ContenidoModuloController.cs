using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class ContenidoModuloController : Controller
{
    private readonly AtriumDbContext _context;

    public ContenidoModuloController(AtriumDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? moduloId)
    {
        var contents = _context.ContenidosModulo
            .AsNoTracking()
            .Include(content => content.ModuloCurso)
                .ThenInclude(module => module!.Curso)
            .AsQueryable();

        if (moduloId.HasValue)
            contents = contents.Where(content => content.IdModulo == moduloId);

        return View(await contents
            .OrderBy(content => content.ModuloCurso!.Curso!.Nombre)
            .ThenBy(content => content.ModuloCurso!.Orden)
            .ThenBy(content => content.Orden)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ContenidosModulo
            .AsNoTracking()
            .Include(content => content.ModuloCurso)
                .ThenInclude(module => module!.Curso)
            .FirstOrDefaultAsync(content => content.IdContenido == id);

        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(int? moduloId)
    {
        await LoadModulesAsync(moduloId);
        return View(new ContenidoModulo { IdModulo = moduloId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContenidoModulo model)
    {
        await ValidateModuleAsync(model.IdModulo);
        ValidateContentType(model.Tipo);

        if (!ModelState.IsValid)
        {
            await LoadModulesAsync(model.IdModulo);
            return View(model);
        }

        var newContent = new ContenidoModulo
        {
            Titulo = model.Titulo.Trim(),
            Tipo = model.Tipo.Trim(),
            URLContenido = model.URLContenido.Trim(),
            Orden = model.Orden,
            IdModulo = model.IdModulo
        };

        _context.ContenidosModulo.Add(newContent);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Contenido creado.";
        return RedirectToAction(nameof(Index), new { moduloId = newContent.IdModulo });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ContenidosModulo.FindAsync(id);
        if (item is null)
            return NotFound();

        await LoadModulesAsync(item.IdModulo);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContenidoModulo posted)
    {
        if (id != posted.IdContenido)
            return NotFound();

        var current = await _context.ContenidosModulo.FindAsync(id);
        if (current is null)
            return NotFound();

        await ValidateModuleAsync(posted.IdModulo);
        ValidateContentType(posted.Tipo);

        if (!ModelState.IsValid)
        {
            await LoadModulesAsync(posted.IdModulo);
            return View(posted);
        }

        current.Titulo = posted.Titulo.Trim();
        current.Tipo = posted.Tipo.Trim();
        current.URLContenido = posted.URLContenido.Trim();
        current.Orden = posted.Orden;
        current.IdModulo = posted.IdModulo;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Contenido actualizado.";

        return RedirectToAction(nameof(Index), new { moduloId = posted.IdModulo });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ContenidosModulo
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.IdContenido == id);

        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.ContenidosModulo.FindAsync(id);
        if (item is not null)
        {
            var progress = _context.ProgresosEstudiante.Where(entry => entry.IdContenido == id);
            _context.ProgresosEstudiante.RemoveRange(progress);
            _context.ContenidosModulo.Remove(item);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Contenido eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadModulesAsync(int? selectedId = null)
    {
        var modules = await _context.ModulosCurso
            .AsNoTracking()
            .Include(module => module.Curso)
            .OrderBy(module => module.Curso!.Nombre)
            .ThenBy(module => module.Orden)
            .Select(module => new
            {
                module.IdModulo,
                Nombre = module.Curso!.Nombre + " · " + module.Titulo
            })
            .ToListAsync();

        ViewData["IdModulo"] = new SelectList(modules, "IdModulo", "Nombre", selectedId);
    }

    private async Task ValidateModuleAsync(int moduleId)
    {
        if (moduleId <= 0 || !await _context.ModulosCurso.AsNoTracking().AnyAsync(module => module.IdModulo == moduleId))
            ModelState.AddModelError(nameof(ContenidoModulo.IdModulo), "Selecciona un módulo válido.");
    }

    private void ValidateContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)
            || !ModuleContentTypes.All.Contains(contentType.Trim(), StringComparer.Ordinal))
        {
            ModelState.AddModelError(nameof(ContenidoModulo.Tipo), "Selecciona un tipo de contenido válido.");
        }
    }
}
