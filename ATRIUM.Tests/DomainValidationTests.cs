using ATRIUM.Domain.Models;
using ATRIUM.Tests.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class DomainValidationTests
{
    // QA: Qué probamos: categoría con longitudes exactamente en sus máximos.
    // QA: Esperado: es válida.
    [TestMethod]
    public void Categoria_LongitudesEnLimite_EsValida()
    {
        var model = new Categoria
        {
            Nombre = new string('N', 100),
            Descripcion = new string('D', 300)
        };

        Assert.AreEqual(0, TestHelpers.Validate(model).Count);
    }

    // QA: Qué probamos: categoría con nombre y descripción sobre el límite.
    // QA: Esperado: ambas propiedades son rechazadas.
    [TestMethod]
    public void Categoria_LongitudesSobreLimite_NoEsValida()
    {
        var model = new Categoria
        {
            Nombre = new string('N', 101),
            Descripcion = new string('D', 301)
        };

        var results = TestHelpers.Validate(model);
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(Categoria.Nombre)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(Categoria.Descripcion)));
    }

    // QA: Qué probamos: carrito con cantidades en los límites 1 y 10.
    // QA: Esperado: ambos límites son válidos.
    [TestMethod]
    public void CarroCompras_CantidadEnLimites_EsValida()
    {
        var lower = new CarroCompras { UsuarioId = "u1", IdCurso = 1, Cantidad = 1 };
        var upper = new CarroCompras { UsuarioId = "u1", IdCurso = 1, Cantidad = 10 };

        Assert.AreEqual(0, TestHelpers.Validate(lower).Count);
        Assert.AreEqual(0, TestHelpers.Validate(upper).Count);
    }

    // QA: Qué probamos: carrito con cantidad cero y once.
    // QA: Esperado: ambos valores quedan fuera del rango permitido.
    [TestMethod]
    public void CarroCompras_CantidadFueraDeLimites_NoEsValida()
    {
        var zero = new CarroCompras { UsuarioId = "u1", IdCurso = 1, Cantidad = 0 };
        var eleven = new CarroCompras { UsuarioId = "u1", IdCurso = 1, Cantidad = 11 };

        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(zero), nameof(CarroCompras.Cantidad)));
        Assert.IsTrue(TestHelpers.HasErrorFor(TestHelpers.Validate(eleven), nameof(CarroCompras.Cantidad)));
    }

    // QA: Qué probamos: contenido de módulo con una URL HTTPS y orden en el máximo.
    // QA: Esperado: es válido.
    [TestMethod]
    public void ContenidoModulo_UrlSeguraYOrdenMaximo_EsValido()
    {
        var model = new ContenidoModulo
        {
            Titulo = "Lección",
            Tipo = "Video",
            URLContenido = "https://example.com/video",
            Orden = 999,
            IdModulo = 1
        };

        Assert.AreEqual(0, TestHelpers.Validate(model).Count);
    }

    // QA: Qué probamos: contenido de módulo con javascript: y orden 1000.
    // QA: Esperado: URLContenido y Orden son rechazados.
    [TestMethod]
    public void ContenidoModulo_UrlPeligrosaYOrdenFueraDeRango_NoEsValido()
    {
        var model = new ContenidoModulo
        {
            Titulo = "Lección",
            Tipo = "Video",
            URLContenido = "javascript:alert(1)",
            Orden = 1000,
            IdModulo = 1
        };

        var results = TestHelpers.Validate(model);
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(ContenidoModulo.URLContenido)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(ContenidoModulo.Orden)));
    }

    // QA: Qué probamos: precios negativos en la entidad Curso.
    // QA: Esperado: Precio y PrecioDescuento son rechazados.
    [TestMethod]
    public void Curso_PreciosNegativos_NoEsValido()
    {
        var model = new Curso
        {
            Nombre = "Curso",
            Precio = -1,
            PrecioDescuento = -1,
            IdCategoria = 1
        };

        var results = TestHelpers.Validate(model);
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(Curso.Precio)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(Curso.PrecioDescuento)));
    }

    // QA: Qué probamos: cálculo de PrecioFinal con un descuento válido.
    // QA: Esperado: se usa el precio descontado.
    [TestMethod]
    public void Curso_PrecioFinal_RespetaDescuentoValido()
    {
        var curso = new Curso { Precio = 200m, PrecioDescuento = 150m };
        Assert.AreEqual(150m, curso.PrecioFinal);
    }

    // QA: Qué probamos: descuentos cero, iguales o superiores al precio.
    // QA: Esperado: PrecioFinal conserva el precio regular.
    [TestMethod]
    public void Curso_PrecioFinal_IgnoraDescuentosNoAplicables()
    {
        Assert.AreEqual(200m, new Curso { Precio = 200m, PrecioDescuento = 0m }.PrecioFinal);
        Assert.AreEqual(200m, new Curso { Precio = 200m, PrecioDescuento = 200m }.PrecioFinal);
        Assert.AreEqual(200m, new Curso { Precio = 200m, PrecioDescuento = 250m }.PrecioFinal);
    }

    // QA: Qué probamos: banner con campos obligatorios vacíos.
    // QA: Esperado: Titulo y Descripcion son rechazados.
    [TestMethod]
    public void Carousel_TextosVacios_NoEsValido()
    {
        var model = new Carousel { Titulo = string.Empty, Descripcion = string.Empty };
        var results = TestHelpers.Validate(model);

        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(Carousel.Titulo)));
        Assert.IsTrue(TestHelpers.HasErrorFor(results, nameof(Carousel.Descripcion)));
    }
}
