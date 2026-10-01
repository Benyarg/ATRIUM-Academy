using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class ModuloCursoController : Controller
{
    private readonly AtriumDbContext _context;

    public ModuloCursoController(AtriumDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int? cursoId)
    {
        var modules = _context.ModulosCurso
            .AsNoTracking()
            .Include(module => module.Curso)
            .AsQueryable();

        if (cursoId.HasValue)
            modules = modules.Where(module => module.IdCurso == cursoId);

        return View(await modules
            .OrderBy(module => module.Curso!.Nombre)
            .ThenBy(module => module.Orden)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ModulosCurso
            .AsNoTracking()
            .Include(module => module.Curso)
            .Include(module => module.Contenidos)
            .FirstOrDefaultAsync(module => module.IdModulo == id);

        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(int? cursoId)
    {
        await LoadCoursesAsync(cursoId);
        return View(new ModuloCurso { IdCurso = cursoId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ModuloCurso model)
    {
        await ValidateCourseAsync(model.IdCurso);
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(model.IdCurso);
            return View(model);
        }

        var newModule = new ModuloCurso
        {
            Titulo = model.Titulo.Trim(),
            Descripcion = model.Descripcion?.Trim(),
            Orden = model.Orden,
            IdCurso = model.IdCurso
        };

        _context.ModulosCurso.Add(newModule);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Módulo creado.";
        return RedirectToAction(nameof(Index), new { cursoId = newModule.IdCurso });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ModulosCurso.FindAsync(id);
        if (item is null)
            return NotFound();

        await LoadCoursesAsync(item.IdCurso);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ModuloCurso posted)
    {
        if (id != posted.IdModulo)
            return NotFound();

        var current = await _context.ModulosCurso.FindAsync(id);
        if (current is null)
            return NotFound();

        await ValidateCourseAsync(posted.IdCurso);
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(posted.IdCurso);
            return View(posted);
        }

        current.Titulo = posted.Titulo.Trim();
        current.Descripcion = posted.Descripcion?.Trim();
        current.Orden = posted.Orden;
        current.IdCurso = posted.IdCurso;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Módulo actualizado.";

        return RedirectToAction(nameof(Index), new { cursoId = posted.IdCurso });
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ModulosCurso
            .AsNoTracking()
            .FirstOrDefaultAsync(module => module.IdModulo == id);

        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _context.ModulosCurso
            .Where(module => module.IdModulo == id)
            .Select(module => new
            {
                Module = module,
                HasContents = module.Contenidos.Any()
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return RedirectToAction(nameof(Index));

        if (result.HasContents)
        {
            TempData["ErrorMessage"] = "Elimina primero los contenidos de este módulo.";
            return RedirectToAction(nameof(Index));
        }

        var courseId = result.Module.IdCurso;
        _context.ModulosCurso.Remove(result.Module);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Módulo eliminado.";
        return RedirectToAction(nameof(Index), new { cursoId = courseId });
    }

    private async Task LoadCoursesAsync(int? selectedId = null)
    {
        var courses = await _context.Cursos
            .AsNoTracking()
            .OrderBy(course => course.Nombre)
            .Select(course => new { course.IdCurso, course.Nombre })
            .ToListAsync();

        ViewData["IdCurso"] = new SelectList(courses, "IdCurso", "Nombre", selectedId);
    }

    private async Task ValidateCourseAsync(int courseId)
    {
        if (courseId <= 0 || !await _context.Cursos.AsNoTracking().AnyAsync(course => course.IdCurso == courseId))
            ModelState.AddModelError(nameof(ModuloCurso.IdCurso), "Selecciona un curso válido.");
    }
}
