using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ATRIUM.Web.Controllers;

public class CertificadoController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly UserManager<Usuario> _userManager;
    private readonly ICourseAccessService _courseAccess;

    public CertificadoController(
        AtriumDbContext context,
        UserManager<Usuario> userManager,
        ICourseAccessService courseAccess)
    {
        _context = context;
        _userManager = userManager;
        _courseAccess = courseAccess;
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator)]
    public async Task<IActionResult> Index()
    {
        var certificates = _context.Certificados
            .AsNoTracking()
            .Include(certificate => certificate.Curso)
            .Include(certificate => certificate.Usuario)
            .AsQueryable();

        if (!User.IsInRole(AppRoles.Administrator))
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            certificates = certificates.Where(certificate => certificate.IdUsuario == userId);
        }

        return View(await certificates
            .OrderByDescending(certificate => certificate.FechaEmision)
            .ToListAsync());
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator)]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var certificate = await _context.Certificados
            .AsNoTracking()
            .Include(item => item.Curso)
            .Include(item => item.Usuario)
            .FirstOrDefaultAsync(item => item.IdCertificado == id);

        if (certificate is null)
            return NotFound();

        if (!User.IsInRole(AppRoles.Administrator)
            && certificate.IdUsuario != _userManager.GetUserId(User))
        {
            return Forbid();
        }

        return View(certificate);
    }

    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Validar(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return View();

        var normalizedCode = codigo.Trim().ToUpperInvariant();
        var certificate = await _context.Certificados
            .AsNoTracking()
            .Include(item => item.Curso)
            .Include(item => item.Usuario)
            .FirstOrDefaultAsync(item => item.CodigoUnico == normalizedCode);

        ViewBag.BusquedaRealizada = true;
        return View(certificate);
    }

    [Authorize(Roles = AppRoles.StudentOrAdministrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Generar(int idCurso)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var course = await _context.Cursos
            .AsNoTracking()
            .Where(item => item.IdCurso == idCurso)
            .Select(item => new
            {
                item.IdCurso,
                TotalContents = item.Modulos.SelectMany(module => module.Contenidos).Count()
            })
            .FirstOrDefaultAsync();

        if (course is null)
            return NotFound();

        var isAdministrator = User.IsInRole(AppRoles.Administrator);
        if (!isAdministrator && !await _courseAccess.HasApprovedAccessAsync(userId, idCurso))
            return Forbid();

        var completedContents = course.TotalContents == 0
            ? 0
            : await _context.ProgresosEstudiante
                .AsNoTracking()
                .CountAsync(progress =>
                    progress.IdUsuario == userId
                    && progress.Completado
                    && progress.ContenidoModulo != null
                    && progress.ContenidoModulo.ModuloCurso != null
                    && progress.ContenidoModulo.ModuloCurso.IdCurso == idCurso);

        var isCompleted = course.TotalContents == 0
                          || completedContents == course.TotalContents;

        if (!isAdministrator && !isCompleted)
        {
            TempData["ErrorMessage"] = "Completa todos los contenidos antes de generar tu certificado.";
            return RedirectToAction("Details", "Curso", new { id = idCurso });
        }

        await using var transaction = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);

        var certificate = await _context.Certificados
            .FirstOrDefaultAsync(
                item => item.IdUsuario == userId && item.IdCurso == idCurso,
                HttpContext.RequestAborted);

        if (certificate is null)
        {
            certificate = new Certificado
            {
                IdCurso = idCurso,
                IdUsuario = userId,
                FechaEmision = DateTime.UtcNow,
                CodigoUnico = GenerateCode(),
                RutaArchivo = string.Empty
            };

            _context.Certificados.Add(certificate);
            await _context.SaveChangesAsync(HttpContext.RequestAborted);
        }

        await transaction.CommitAsync(HttpContext.RequestAborted);
        return RedirectToAction(nameof(Details), new { id = certificate.IdCertificado });
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Create()
    {
        await LoadSelectorsAsync();
        return View(new Certificado { FechaEmision = DateTime.UtcNow });
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Certificado model)
    {
        model.CodigoUnico = string.IsNullOrWhiteSpace(model.CodigoUnico)
            ? GenerateCode()
            : model.CodigoUnico.Trim().ToUpperInvariant();
        model.RutaArchivo = model.RutaArchivo?.Trim() ?? string.Empty;

        await ValidateCertificateAsync(model, excludedId: null);
        if (!ModelState.IsValid)
        {
            await LoadSelectorsAsync(model.IdCurso, model.IdUsuario);
            return View(model);
        }

        var newCertificate = new Certificado
        {
            IdCurso = model.IdCurso,
            IdUsuario = model.IdUsuario,
            FechaEmision = model.FechaEmision,
            CodigoUnico = model.CodigoUnico,
            RutaArchivo = model.RutaArchivo
        };

        _context.Certificados.Add(newCertificate);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Certificado creado.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Certificados.FindAsync(id);
        if (item is null)
            return NotFound();

        return View(item);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Certificado posted)
    {
        if (id != posted.IdCertificado)
            return NotFound();

        var current = await _context.Certificados.FindAsync(id);
        if (current is null)
            return NotFound();

        posted.CodigoUnico = posted.CodigoUnico.Trim().ToUpperInvariant();
        posted.RutaArchivo = posted.RutaArchivo?.Trim() ?? string.Empty;

        await ValidateCertificateAsync(posted, id);
        if (!ModelState.IsValid)
            return View(posted);

        current.IdCurso = posted.IdCurso;
        current.IdUsuario = posted.IdUsuario;
        current.FechaEmision = posted.FechaEmision;
        current.CodigoUnico = posted.CodigoUnico;
        current.RutaArchivo = posted.RutaArchivo;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Certificado actualizado.";

        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = AppRoles.Administrator)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Certificados
            .AsNoTracking()
            .Include(certificate => certificate.Curso)
            .FirstOrDefaultAsync(certificate => certificate.IdCertificado == id);

        return item is null ? NotFound() : View(item);
    }

    [Authorize(Roles = AppRoles.Administrator), HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Certificados.FindAsync(id);
        if (item is not null)
        {
            _context.Certificados.Remove(item);
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Certificado eliminado.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCertificateAsync(Certificado model, int? excludedId)
    {
        if (!await _context.Cursos.AsNoTracking().AnyAsync(course => course.IdCurso == model.IdCurso))
            ModelState.AddModelError(nameof(model.IdCurso), "El curso seleccionado no existe.");

        if (!await _context.Users.AsNoTracking().AnyAsync(user => user.Id == model.IdUsuario))
            ModelState.AddModelError(nameof(model.IdUsuario), "El usuario seleccionado no existe.");

        if (await _context.Certificados.AsNoTracking().AnyAsync(certificate =>
                certificate.IdUsuario == model.IdUsuario
                && certificate.IdCurso == model.IdCurso
                && (!excludedId.HasValue || certificate.IdCertificado != excludedId.Value)))
        {
            ModelState.AddModelError(string.Empty, "Ese usuario ya tiene un certificado para el curso seleccionado.");
        }

        if (await _context.Certificados.AsNoTracking().AnyAsync(certificate =>
                certificate.CodigoUnico == model.CodigoUnico
                && (!excludedId.HasValue || certificate.IdCertificado != excludedId.Value)))
        {
            ModelState.AddModelError(nameof(model.CodigoUnico), "El código ya está en uso.");
        }
    }

    private async Task LoadSelectorsAsync(int? selectedCourseId = null, string? selectedUserId = null)
    {
        var courses = await _context.Cursos
            .AsNoTracking()
            .OrderBy(course => course.Nombre)
            .Select(course => new { course.IdCurso, course.Nombre })
            .ToListAsync();

        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Select(user => new { user.Id, user.Email })
            .ToListAsync();

        ViewData["IdCurso"] = new SelectList(courses, "IdCurso", "Nombre", selectedCourseId);
        ViewData["IdUsuario"] = new SelectList(users, "Id", "Email", selectedUserId);
    }

    private static string GenerateCode() =>
        (CertificateDefaults.CodePrefix + Guid.NewGuid().ToString("N")[..14]).ToUpperInvariant();
}
