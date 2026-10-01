using ATRIUM.Domain.Models;
using ATRIUM.Infrastructure.Context;
using ATRIUM.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class DbContextModelTests
{
    // QA: Qué probamos: persistencia básica de categoría y curso en el contexto.
    // QA: Esperado: ambas entidades se guardan y pueden contarse.
    [TestMethod]
    public async Task Contexto_PermiteCrearCategoriaYCurso()
    {
        await using var context = TestHelpers.CreateContext();
        var categoria = new Categoria { Nombre = "BIM", Descripcion = "Diseño y modelado", Estado = true };
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();

        context.Cursos.Add(new Curso
        {
            Nombre = "Introducción a BIM",
            Descripcion = "Curso de prueba",
            Precio = 120m,
            PrecioDescuento = 90m,
            IdCategoria = categoria.Id
        });
        await context.SaveChangesAsync();

        Assert.AreEqual(1, await context.Categorias.CountAsync());
        Assert.AreEqual(1, await context.Cursos.CountAsync());
    }

    // QA: Qué probamos: índice único de Categoria.Nombre.
    // QA: Esperado: el modelo EF lo marca como único.
    [TestMethod]
    public void Modelo_CategoriaNombre_TieneIndiceUnico()
    {
        using var context = TestHelpers.CreateContext();
        var entity = context.Model.FindEntityType(typeof(Categoria))!;
        var index = entity.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(Categoria.Nombre)]));

        Assert.IsTrue(index.IsUnique);
    }

    // QA: Qué probamos: unicidad de UsuarioId + IdCurso en carrito.
    // QA: Esperado: el índice compuesto es único.
    [TestMethod]
    public void Modelo_CarritoUsuarioCurso_TieneIndiceUnico()
    {
        using var context = TestHelpers.CreateContext();
        AssertCompositeUniqueIndex(context, typeof(CarroCompras), nameof(CarroCompras.UsuarioId), nameof(CarroCompras.IdCurso));
    }

    // QA: Qué probamos: unicidad de Usuario + Contenido en progreso.
    // QA: Esperado: evita dos registros de progreso para el mismo contenido y usuario.
    [TestMethod]
    public void Modelo_ProgresoUsuarioContenido_TieneIndiceUnico()
    {
        using var context = TestHelpers.CreateContext();
        AssertCompositeUniqueIndex(context, typeof(ProgresoEstudiante), nameof(ProgresoEstudiante.IdUsuario), nameof(ProgresoEstudiante.IdContenido));
    }

    // QA: Qué probamos: índices únicos de certificado.
    // QA: Esperado: CodigoUnico y Usuario+Curso son únicos.
    [TestMethod]
    public void Modelo_Certificado_TieneIndicesUnicosEsperados()
    {
        using var context = TestHelpers.CreateContext();
        var entity = context.Model.FindEntityType(typeof(Certificado))!;

        Assert.IsTrue(entity.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(Certificado.CodigoUnico)])).IsUnique);
        Assert.IsTrue(entity.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(Certificado.IdUsuario), nameof(Certificado.IdCurso)])).IsUnique);
    }

    // QA: Qué probamos: comportamiento de borrado Curso -> Categoria.
    // QA: Esperado: DeleteBehavior.Restrict evita eliminaciones en cascada accidentales.
    [TestMethod]
    public void Modelo_CursoCategoria_UsaDeleteRestrict()
    {
        using var context = TestHelpers.CreateContext();
        var entity = context.Model.FindEntityType(typeof(Curso))!;
        var fk = entity.GetForeignKeys().Single(key => key.PrincipalEntityType.ClrType == typeof(Categoria));

        Assert.AreEqual(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    // QA: Qué probamos: precisión monetaria declarada para Curso y Pedido.
    // QA: Esperado: precisión 18 y escala 2.
    [TestMethod]
    public void Modelo_DecimalesMonetarios_UsanPrecision18Escala2()
    {
        using var context = TestHelpers.CreateContext();

        AssertPrecision(context, typeof(Curso), nameof(Curso.Precio));
        AssertPrecision(context, typeof(Curso), nameof(Curso.PrecioDescuento));
        AssertPrecision(context, typeof(Pedido), nameof(Pedido.TotalPedido));
        AssertPrecision(context, typeof(PedidoDetalle), nameof(PedidoDetalle.PrecioIndividual));
    }

    private static void AssertCompositeUniqueIndex(AtriumDbContext context, Type entityType, params string[] properties)
    {
        var entity = context.Model.FindEntityType(entityType)!;
        var index = entity.GetIndexes().Single(i => i.Properties.Select(p => p.Name).SequenceEqual(properties));
        Assert.IsTrue(index.IsUnique);
    }

    private static void AssertPrecision(AtriumDbContext context, Type entityType, string propertyName)
    {
        var property = context.Model.FindEntityType(entityType)!.FindProperty(propertyName)!;
        Assert.AreEqual(18, property.GetPrecision());
        Assert.AreEqual(2, property.GetScale());
    }
}
