using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.Services;
using ATRIUM.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ATRIUM.Web.Controllers;

public class HomeController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly IContactEmailService _contactEmailService;
    private readonly ILogger<HomeController> _logger;

    /*
     * Valores permitidos desde el formulario de contacto.
     * Evita almacenar categorías arbitrarias manipuladas
     * desde el navegador.
     */
    private static readonly HashSet<string> AllowedContactTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Cursos",
            "Pedidos",
            "Acceso",
            "Recursos",
            "Progreso",
            "Certificados",
            "Cuenta",
            "Otro"
        };

    public HomeController(
        AtriumDbContext context,
        IContactEmailService contactEmailService,
        ILogger<HomeController> logger)
    {
        _context = context;
        _contactEmailService = contactEmailService;
        _logger = logger;
    }

    // =========================================================
    // INICIO
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var featuredCourses = await _context.Cursos
            .AsNoTracking()
            .Include(course => course.Categoria)
            .OrderByDescending(course => course.IdCurso)
            .Take(6)
            .ToListAsync(cancellationToken);

        var banners = await _context.Carouseles
            .AsNoTracking()
            .Where(banner => banner.Activo)
            .OrderBy(banner => banner.Orden)
            .Take(4)
            .ToListAsync(cancellationToken);

        var categories = await _context.Categorias
            .AsNoTracking()
            .Where(category => category.Estado)
            .OrderBy(category => category.Nombre)
            .Select(category => new HomeCategoriaViewModel
            {
                Id = category.Id,
                Nombre = category.Nombre,
                Descripcion = category.Descripcion,
                TotalCursos = category.Cursos.Count
            })
            .Take(6)
            .ToListAsync(cancellationToken);

        var totalCursos = await _context.Cursos
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var totalRecursos = await _context.ContenidosEducativos
            .AsNoTracking()
            .CountAsync(
                resource => resource.EsActivo,
                cancellationToken);

        var model = new HomeIndexViewModel
        {
            CursosDestacados = featuredCourses,
            Banners = banners,
            Categorias = categories,
            TotalCursos = totalCursos,
            TotalRecursos = totalRecursos
        };

        return View(model);
    }

    // =========================================================
    // NOSOTROS
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Nosotros()
    {
        return View();
    }

    // =========================================================
    // FAQ
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult FAQ()
    {
        return View();
    }

    // =========================================================
    // CONTACTO - GET
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Contacto()
    {
        var model = new ContactoViewModel();

        if (User.Identity?.IsAuthenticated == true)
        {
            model.Email = GetAuthenticatedEmail();
        }

        return View(model);
    }

    // =========================================================
    // CONTACTO - POST
    // =========================================================

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contacto(
        ContactoViewModel model,
        CancellationToken cancellationToken)
    {
        /*
         * Normalización de los datos recibidos.
         */
        model.Nombre =
            model.Nombre?.Trim() ?? string.Empty;

        model.Email =
            model.Email?.Trim() ?? string.Empty;

        model.TipoConsulta =
            model.TipoConsulta?.Trim() ?? string.Empty;

        model.NumeroPedido =
            string.IsNullOrWhiteSpace(model.NumeroPedido)
                ? null
                : model.NumeroPedido.Trim();

        model.Asunto =
            model.Asunto?.Trim() ?? string.Empty;

        model.Mensaje =
            model.Mensaje?.Trim() ?? string.Empty;

        string? userId = null;

        /*
         * Cuando hay una sesión iniciada, no confiamos
         * en el email enviado desde el HTML.
         *
         * Tomamos tanto el Id como el correo desde
         * la identidad autenticada.
         */
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            model.Email = GetAuthenticatedEmail();
        }

        /*
         * Como modificamos datos después del binding,
         * limpiamos ModelState y validamos nuevamente
         * el modelo normalizado.
         */
        ModelState.Clear();

        TryValidateModel(model);

        /*
         * Validación adicional del tipo de consulta.
         */
        if (!string.IsNullOrWhiteSpace(model.TipoConsulta) &&
            !AllowedContactTypes.Contains(model.TipoConsulta))
        {
            ModelState.AddModelError(
                nameof(model.TipoConsulta),
                "Selecciona un tipo de consulta válido.");
        }

        /*
         * Una sesión autenticada debería contener
         * un correo asociado.
         */
        if (User.Identity?.IsAuthenticated == true &&
            string.IsNullOrWhiteSpace(model.Email))
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "No se pudo identificar el correo asociado a tu cuenta.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // =====================================================
        // CREAR CONSULTA
        // =====================================================

        var consulta = new ConsultaContacto
        {
            UsuarioId = userId,

            Nombre = model.Nombre,
            Email = model.Email,
            TipoConsulta = model.TipoConsulta,
            NumeroPedido = model.NumeroPedido,
            Asunto = model.Asunto,
            Mensaje = model.Mensaje,

            FechaCreacion = DateTime.UtcNow,

            CorreoNotificacionEnviado = false,
            FechaCorreoEnviado = null
        };

        // =====================================================
        // 1. GUARDAR PRIMERO EN SQL SERVER
        // =====================================================

        try
        {
            _context.ConsultasContacto.Add(consulta);

            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(
                ex,
                "No se pudo guardar una consulta de contacto.");

            ModelState.AddModelError(
                string.Empty,
                "No pudimos registrar tu consulta en este momento. Inténtalo nuevamente.");

            return View(model);
        }

        // =====================================================
        // 2. ENVIAR NOTIFICACIÓN POR GMAIL
        // =====================================================

        try
        {
            await _contactEmailService
                .SendContactNotificationAsync(
                    consulta,
                    cancellationToken);

            consulta.CorreoNotificacionEnviado = true;
            consulta.FechaCorreoEnviado = DateTime.UtcNow;

            await _context.SaveChangesAsync(
                cancellationToken);

            TempData["SuccessMessage"] =
                "Tu consulta fue enviada correctamente. Te responderemos al correo indicado.";
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            /*
             * Si el navegador cancela realmente la petición,
             * no ocultamos esa cancelación.
             */
            throw;
        }
        catch (Exception ex)
        {
            /*
             * Gmail puede fallar, pero la consulta ya está
             * almacenada en SQL Server.
             */
            _logger.LogError(
                ex,
                "La consulta {ConsultaId} fue almacenada, pero la notificación por correo no pudo enviarse.",
                consulta.IdConsulta);

            TempData["SuccessMessage"] =
                "Tu consulta fue registrada correctamente. Nuestro equipo podrá revisarla desde ATRIUM.";
        }

        return RedirectToAction(nameof(Contacto));
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private string GetAuthenticatedEmail()
    {
        /*
         * Intentamos primero recuperar específicamente
         * el claim de email.
         *
         * Como respaldo usamos Identity.Name porque
         * actualmente ATRIUM utiliza el correo como UserName.
         */
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name
            ?? string.Empty;
    }

    // =========================================================
    // ERROR
    // =========================================================

    [AllowAnonymous]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId =
                Activity.Current?.Id ??
                HttpContext.TraceIdentifier
        });
    }
}