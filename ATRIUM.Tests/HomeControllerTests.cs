using ATRIUM.Domain.Models;
using ATRIUM.Tests.TestSupport;
using ATRIUM.Web.Controllers;
using ATRIUM.Web.Services;
using ATRIUM.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class HomeControllerTests
{
    // =========================================================
    // TEST 1
    // HOME CON BASE DE DATOS VACÍA
    // =========================================================

    [TestMethod]
    public async Task Index_BaseVacia_DevuelveModeloVacioValido()
    {
        await using var context = TestHelpers.CreateContext();

        var controller = CreateController(context);

        var result = await controller.Index(
            CancellationToken.None);

        Assert.IsInstanceOfType(
            result,
            typeof(ViewResult));

        var viewResult = (ViewResult)result;

        Assert.IsNotNull(viewResult.Model);

        var model =
            (HomeIndexViewModel)viewResult.Model;

        Assert.AreEqual(
            0,
            model.TotalCursos);

        Assert.AreEqual(
            0,
            model.TotalRecursos);

        Assert.AreEqual(
            0,
            model.CursosDestacados.Count);

        Assert.AreEqual(
            0,
            model.Banners.Count);

        Assert.AreEqual(
            0,
            model.Categorias.Count);
    }

    // =========================================================
    // TEST 2
    // HOME CON DATOS ACTIVOS E INACTIVOS
    // =========================================================

    [TestMethod]
    public async Task Index_DatosMixtos_FiltraActivosYCuentaCorrectamente()
    {
        await using var context =
            TestHelpers.CreateContext();

        // -----------------------------------------------------
        // CATEGORÍAS
        // -----------------------------------------------------

        var activeCategory = new Categoria
        {
            Nombre = "BIM",
            Descripcion = "Activa",
            Estado = true
        };

        var inactiveCategory = new Categoria
        {
            Nombre = "Legacy",
            Descripcion = "Inactiva",
            Estado = false
        };

        context.Categorias.AddRange(
            activeCategory,
            inactiveCategory);

        await context.SaveChangesAsync();

        // -----------------------------------------------------
        // CURSO
        // -----------------------------------------------------

        context.Cursos.Add(
            new Curso
            {
                Nombre = "Curso BIM",
                Precio = 100,
                IdCategoria = activeCategory.Id
            });

        // -----------------------------------------------------
        // BANNERS
        // -----------------------------------------------------

        context.Carouseles.AddRange(
            new Carousel
            {
                Titulo = "Activo",
                Descripcion = "Visible",
                Activo = true,
                Orden = 1
            },
            new Carousel
            {
                Titulo = "Inactivo",
                Descripcion = "Oculto",
                Activo = false,
                Orden = 2
            });

        // -----------------------------------------------------
        // RECURSOS
        // -----------------------------------------------------

        context.ContenidosEducativos.AddRange(
            new ContenidoEducativo
            {
                Titulo = "Activo",
                Descripcion = "Visible",
                CursoId = 1,
                EsActivo = true
            },
            new ContenidoEducativo
            {
                Titulo = "Inactivo",
                Descripcion = "Oculto",
                CursoId = 1,
                EsActivo = false
            });

        await context.SaveChangesAsync();

        // -----------------------------------------------------
        // CONTROLADOR
        // -----------------------------------------------------

        var controller =
            CreateController(context);

        var result = await controller.Index(
            CancellationToken.None);

        Assert.IsInstanceOfType(
            result,
            typeof(ViewResult));

        var viewResult =
            (ViewResult)result;

        Assert.IsNotNull(
            viewResult.Model);

        var model =
            (HomeIndexViewModel)viewResult.Model;

        // -----------------------------------------------------
        // VALIDACIONES
        // -----------------------------------------------------

        Assert.AreEqual(
            1,
            model.TotalCursos);

        Assert.AreEqual(
            1,
            model.TotalRecursos);

        Assert.AreEqual(
            1,
            model.Banners.Count);

        Assert.AreEqual(
            "Activo",
            model.Banners.Single().Titulo);

        Assert.AreEqual(
            1,
            model.Categorias.Count);

        Assert.AreEqual(
            "BIM",
            model.Categorias.Single().Nombre);

        Assert.AreEqual(
            1,
            model.Categorias.Single().TotalCursos);
    }

    // =========================================================
    // FACTORY DEL HOME CONTROLLER
    // =========================================================

    private static HomeController CreateController(
        ATRIUM.Infrastructure.Context.AtriumDbContext context)
    {
        /*
         * En las pruebas no queremos conectarnos
         * realmente a Gmail.
         */
        var emailService =
            new FakeContactEmailService();

        /*
         * Logger vacío para pruebas.
         */
        var logger =
            NullLogger<HomeController>.Instance;

        return new HomeController(
            context,
            emailService,
            logger);
    }

    // =========================================================
    // EMAIL SERVICE FALSO PARA TESTS
    // =========================================================

    private sealed class FakeContactEmailService
        : IContactEmailService
    {
        public Task SendContactNotificationAsync(
            ConsultaContacto consulta,
            CancellationToken cancellationToken = default)
        {
            /*
             * No hacemos ninguna conexión SMTP.
             *
             * Simulamos que el envío terminó
             * correctamente.
             */
            return Task.CompletedTask;
        }
    }
}