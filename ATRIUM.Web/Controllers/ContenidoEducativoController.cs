using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.StudentOrAdministrator)]
public class ContenidoEducativoController : Controller
{
    private readonly AtriumDbContext _context;

    public ContenidoEducativoController(AtriumDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var resources = _context.ContenidosEducativos
            .AsNoTracking()
            .Include(resource => resource.Curso)
            .AsQueryable();

        if (!User.IsInRole(AppRoles.Administrator))
            resources = resources.Where(resource => resource.EsActivo);

        return View(await resources
            .OrderByDescending(resource => resource.FechaPublicacion)
            .ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var resources = _context.ContenidosEducativos
            .AsNoTracking()
            .Include(resource => resource.Curso)
            .AsQueryable();

        if (!User.IsInRole(AppRoles.Administrator))
            resources = resources.Where(resource => resource.EsActivo);

        var item = await resources.FirstOrDefaultAsync(resource => resource.ContenidoId == id);
        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Create()
    {
        await LoadCoursesAsync();
        return View(new ContenidoEducativo());
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContenidoEducativo model)
    {
        await ValidateCourseAsync(model.CursoId);
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(model.CursoId);
            return View(model);
        }

        var newResource = new ContenidoEducativo
        {
            Titulo = model.Titulo.Trim(),
            Descripcion = model.Descripcion.Trim(),
            UrlContenido = model.UrlContenido?.Trim(),
            CursoId = model.CursoId,
            FechaPublicacion = DateTime.UtcNow,
            EsActivo = model.EsActivo
        };

        _context.ContenidosEducativos.Add(newResource);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Recurso publicado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ContenidosEducativos.FindAsync(id);
        if (item is null)
            return NotFound();

        await LoadCoursesAsync(item.CursoId);
        return View(item);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ContenidoEducativo posted)
    {
        if (id != posted.ContenidoId)
            return NotFound();

        var current = await _context.ContenidosEducativos.FindAsync(id);
        if (current is null)
            return NotFound();

        await ValidateCourseAsync(posted.CursoId);
        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(posted.CursoId);
            return View(posted);
        }

        current.Titulo = posted.Titulo.Trim();
        current.Descripcion = posted.Descripcion.Trim();
        current.UrlContenido = posted.UrlContenido?.Trim();
        current.CursoId = posted.CursoId;
        current.EsActivo = posted.EsActivo;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Recurso actualizado.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.ContenidosEducativos
            .AsNoTracking()
            .FirstOrDefaultAsync(resource => resource.ContenidoId == id);

        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.ContenidosEducativos.FindAsync(id);
        if (item is not null)
        {
            _context.ContenidosEducativos.Remove(item);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Recurso eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCoursesAsync(int? selectedId = null)
    {
        var courses = await _context.Cursos
            .AsNoTracking()
            .OrderBy(course => course.Nombre)
            .Select(course => new { course.IdCurso, course.Nombre })
            .ToListAsync();

        ViewData["CursoId"] = new SelectList(courses, "IdCurso", "Nombre", selectedId);
    }

    private async Task ValidateCourseAsync(int courseId)
    {
        if (courseId <= 0 || !await _context.Cursos.AsNoTracking().AnyAsync(course => course.IdCurso == courseId))
            ModelState.AddModelError(nameof(ContenidoEducativo.CursoId), "Selecciona un curso válido.");
    }
}
