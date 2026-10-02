using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class CarouselController : Controller
{
    private readonly AtriumDbContext _context;
    private readonly IImageStorageService _imageStorage;

    public CarouselController(
        AtriumDbContext context,
        IImageStorageService imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

    public async Task<IActionResult> Index() =>
        View(await _context.Carouseles
            .AsNoTracking()
            .OrderBy(carousel => carousel.Orden)
            .ToListAsync());

    public async Task<IActionResult> Create()
    {
        var currentMaxOrder = await _context.Carouseles
            .Select(item => (int?)item.Orden)
            .MaxAsync() ?? 0;

        return View(new Carousel
        {
            Activo = true,
            Orden = currentMaxOrder + 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Carousel carousel,
        IFormFile? imagen)
    {
        var imageError = await _imageStorage.ValidateAsync(
            imagen,
            required: true,
            HttpContext.RequestAborted);

        if (imageError is not null)
        {
            ModelState.AddModelError(
                nameof(carousel.ImagenUrl),
                imageError);
        }

        if (!ModelState.IsValid)
            return View(carousel);

        var imagePath = await _imageStorage.SaveAsync(
            imagen!,
            ImageStorageArea.Carousel,
            HttpContext.RequestAborted);

        var newCarousel = new Carousel
        {
            Titulo = carousel.Titulo.Trim(),
            Descripcion = carousel.Descripcion.Trim(),
            ImagenUrl = imagePath,
            Activo = carousel.Activo,
            Orden = carousel.Orden
        };

        _context.Carouseles.Add(newCarousel);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _imageStorage.DeleteAsync(
                imagePath,
                ImageStorageArea.Carousel,
                HttpContext.RequestAborted);

            throw;
        }

        TempData["SuccessMessage"] = "Banner creado.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Carouseles
            .AsNoTracking()
            .FirstOrDefaultAsync(carousel => carousel.Id == id);

        return item is null
            ? NotFound()
            : View(item);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Carouseles.FindAsync(id);

        return item is null
            ? NotFound()
            : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Carousel posted,
        IFormFile? nuevaImagen)
    {
        if (id != posted.Id)
            return NotFound();

        var current = await _context.Carouseles.FindAsync(id);

        if (current is null)
            return NotFound();

        ModelState.Remove(nameof(posted.ImagenUrl));

        var imageError = await _imageStorage.ValidateAsync(
            nuevaImagen,
            required: false,
            HttpContext.RequestAborted);

        if (imageError is not null)
        {
            ModelState.AddModelError(
                nameof(posted.ImagenUrl),
                imageError);
        }

        if (!ModelState.IsValid)
        {
            posted.ImagenUrl = current.ImagenUrl;

            return View(posted);
        }

        current.Titulo = posted.Titulo.Trim();
        current.Descripcion = posted.Descripcion.Trim();
        current.Orden = posted.Orden;
        current.Activo = posted.Activo;

        var previousImagePath = current.ImagenUrl;

        string? newImagePath = null;

        if (nuevaImagen is not null)
        {
            newImagePath = await _imageStorage.SaveAsync(
                nuevaImagen,
                ImageStorageArea.Carousel,
                HttpContext.RequestAborted);

            current.ImagenUrl = newImagePath;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch
        {
            await _imageStorage.DeleteAsync(
                newImagePath,
                ImageStorageArea.Carousel,
                HttpContext.RequestAborted);

            throw;
        }

        if (newImagePath is not null)
        {
            await _imageStorage.DeleteAsync(
                previousImagePath,
                ImageStorageArea.Carousel,
                HttpContext.RequestAborted);
        }

        TempData["SuccessMessage"] = "Banner actualizado.";

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var item = await _context.Carouseles
            .AsNoTracking()
            .FirstOrDefaultAsync(carousel => carousel.Id == id);

        return item is null
            ? NotFound()
            : View(item);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Carouseles.FindAsync(id);

        if (item is not null)
        {
            var imagePath = item.ImagenUrl;

            _context.Carouseles.Remove(item);

            await _context.SaveChangesAsync();

            await _imageStorage.DeleteAsync(
                imagePath,
                ImageStorageArea.Carousel,
                HttpContext.RequestAborted);
        }

        TempData["SuccessMessage"] = "Banner eliminado.";

        return RedirectToAction(nameof(Index));
    }
}