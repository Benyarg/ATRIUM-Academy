using ATRIUM.Domain.Models;
using ATRIUM.Tests.TestSupport;
using ATRIUM.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class CategoriaControllerTests
{
    // QA: Qué probamos: Details sin identificador.
    // QA: Esperado: responde NotFound y no consulta una categoría arbitraria.
    [TestMethod]
    public async Task Details_IdNulo_DevuelveNotFound()
    {
        await using var context = TestHelpers.CreateContext();
        var controller = TestHelpers.ConfigureController(new CategoriaController(context));

        Assert.IsInstanceOfType(await controller.Details(null), typeof(NotFoundResult));
    }

    // QA: Qué probamos: creación válida con espacios al inicio y final.
    // QA: Esperado: se guarda el texto recortado y redirige al Index.
    [TestMethod]
    public async Task Create_DatosValidos_RecortaYGuardaCategoria()
    {
        await using var context = TestHelpers.CreateContext();
        var controller = TestHelpers.ConfigureController(new CategoriaController(context));
        var posted = new Categoria { Nombre = "  BIM  ", Descripcion = "  Modelado  ", Estado = true };

        var result = await controller.Create(posted);
        var stored = await context.Categorias.SingleAsync();

        Assert.AreEqual("BIM", stored.Nombre);
        Assert.AreEqual("Modelado", stored.Descripcion);
        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual(nameof(CategoriaController.Index), ((RedirectToActionResult)result).ActionName);
    }

    // QA: Qué probamos: creación de una categoría duplicada después de aplicar Trim.
    // QA: Esperado: vuelve a la vista con error y no inserta un segundo registro.
    [TestMethod]
    public async Task Create_NombreDuplicado_NoGuarda()
    {
        await using var context = TestHelpers.CreateContext();
        context.Categorias.Add(new Categoria { Nombre = "BIM", Descripcion = "Primera" });
        await context.SaveChangesAsync();

        var controller = TestHelpers.ConfigureController(new CategoriaController(context));
        var result = await controller.Create(new Categoria { Nombre = " BIM ", Descripcion = "Duplicada" });

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.IsFalse(controller.ModelState.IsValid);
        Assert.AreEqual(1, await context.Categorias.CountAsync());
    }


    // QA: Qué probamos: valores nulos malformados enviados a Create con ModelState inválido.
    // QA: Esperado: no lanza NullReferenceException, retorna la vista y no guarda datos.
    [TestMethod]
    public async Task Create_ValoresNulosConModelStateInvalido_NoLanzaExcepcionNiGuarda()
    {
        await using var context = TestHelpers.CreateContext();
        var controller = TestHelpers.ConfigureController(new CategoriaController(context));
        controller.ModelState.AddModelError(nameof(Categoria.Nombre), "Requerido");
        controller.ModelState.AddModelError(nameof(Categoria.Descripcion), "Requerido");
        var posted = new Categoria { Nombre = null!, Descripcion = null! };

        var result = await controller.Create(posted);

        Assert.IsInstanceOfType(result, typeof(ViewResult));
        Assert.AreEqual(0, await context.Categorias.CountAsync());
    }

    // QA: Qué probamos: edición donde el Id de la ruta no coincide con el Id publicado.
    // QA: Esperado: devuelve NotFound y no modifica datos.
    [TestMethod]
    public async Task Edit_IdNoCoincide_DevuelveNotFound()
    {
        await using var context = TestHelpers.CreateContext();
        context.Categorias.Add(new Categoria { Id = 1, Nombre = "BIM", Descripcion = "Original" });
        await context.SaveChangesAsync();

        var controller = TestHelpers.ConfigureController(new CategoriaController(context));
        var result = await controller.Edit(1, new Categoria { Id = 2, Nombre = "Otro", Descripcion = "Otro" });

        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        Assert.AreEqual("BIM", (await context.Categorias.SingleAsync()).Nombre);
    }

    // QA: Qué probamos: intento de eliminar categoría que aún contiene cursos.
    // QA: Esperado: no elimina la categoría y establece mensaje de error.
    [TestMethod]
    public async Task DeleteConfirmed_CategoriaConCursos_NoElimina()
    {
        await using var context = TestHelpers.CreateContext();
        var category = new Categoria { Nombre = "BIM", Descripcion = "Categoría" };
        category.Cursos.Add(new Curso { Nombre = "Curso", Precio = 100, IdCategoria = category.Id });
        context.Categorias.Add(category);
        await context.SaveChangesAsync();

        var controller = TestHelpers.ConfigureController(new CategoriaController(context));
        var result = await controller.DeleteConfirmed(category.Id);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual(1, await context.Categorias.CountAsync());
        Assert.IsTrue(controller.TempData.ContainsKey("ErrorMessage"));
    }

    // QA: Qué probamos: eliminación de un Id inexistente.
    // QA: Esperado: operación idempotente; redirige al índice sin excepción.
    [TestMethod]
    public async Task DeleteConfirmed_IdInexistente_RedireccionaSinError()
    {
        await using var context = TestHelpers.CreateContext();
        var controller = TestHelpers.ConfigureController(new CategoriaController(context));

        var result = await controller.DeleteConfirmed(999);

        Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
        Assert.AreEqual(nameof(CategoriaController.Index), ((RedirectToActionResult)result).ActionName);
    }
}
