using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.StudentOrAdministrator)]
public class ProgresoEstudianteController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly ICourseAccessService _courseAccess;

    public ProgresoEstudianteController(
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

        var progress = await _context.ProgresosEstudiante
            .AsNoTracking()
            .Where(item => item.IdUsuario == userId)
            .Include(item => item.ContenidoModulo)
                .ThenInclude(content => content!.ModuloCurso)
                .ThenInclude(module => module!.Curso)
            .OrderByDescending(item => item.FechaCompletado)
            .ToListAsync();

        return View(progress);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Marcar(int idContenido, bool completado = true)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var courseId = await _context.ContenidosModulo
            .AsNoTracking()
            .Where(content => content.IdContenido == idContenido)
            .Select(content => (int?)content.ModuloCurso!.IdCurso)
            .FirstOrDefaultAsync();

        if (!courseId.HasValue)
            return NotFound();

        var hasAccess = User.IsInRole(AppRoles.Administrator)
                        || await _courseAccess.HasApprovedAccessAsync(userId, courseId.Value);

        if (!hasAccess)
            return Forbid();

        var progress = await _context.ProgresosEstudiante
            .FirstOrDefaultAsync(item => item.IdUsuario == userId && item.IdContenido == idContenido);

        if (progress is null)
        {
            progress = new ProgresoEstudiante
            {
                IdUsuario = userId,
                IdContenido = idContenido
            };
            _context.ProgresosEstudiante.Add(progress);
        }

        progress.Completado = completado;
        progress.FechaCompletado = completado ? DateTime.UtcNow : DateTime.MinValue;

        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Curso", new { id = courseId.Value });
    }
}
