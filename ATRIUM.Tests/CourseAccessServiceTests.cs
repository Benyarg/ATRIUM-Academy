using ATRIUM.Domain.Constants;
using ATRIUM.Domain.Models;
using ATRIUM.Tests.TestSupport;
using ATRIUM.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATRIUM.Tests;

[TestClass]
public class CourseAccessServiceTests
{
    // QA: Qué probamos: pedido aprobado del mismo usuario y curso.
    // QA: Esperado: HasApprovedAccessAsync devuelve true.
    [TestMethod]
    public async Task HasApprovedAccessAsync_PedidoAprobadoMismoUsuarioYCurso_DevuelveTrue()
    {
        await using var context = TestHelpers.CreateContext();
        await SeedOrderAsync(context, "user-1", 10, OrderStatuses.Approved);
        var service = new CourseAccessService(context);

        Assert.IsTrue(await service.HasApprovedAccessAsync("user-1", 10));
    }

    // QA: Qué probamos: pedido aprobado perteneciente a otro usuario.
    // QA: Esperado: no concede acceso.
    [TestMethod]
    public async Task HasApprovedAccessAsync_OtroUsuario_DevuelveFalse()
    {
        await using var context = TestHelpers.CreateContext();
        await SeedOrderAsync(context, "user-2", 10, OrderStatuses.Approved);
        var service = new CourseAccessService(context);

        Assert.IsFalse(await service.HasApprovedAccessAsync("user-1", 10));
    }

    // QA: Qué probamos: pedido aprobado para otro curso.
    // QA: Esperado: no concede acceso al curso consultado.
    [TestMethod]
    public async Task HasApprovedAccessAsync_OtroCurso_DevuelveFalse()
    {
        await using var context = TestHelpers.CreateContext();
        await SeedOrderAsync(context, "user-1", 20, OrderStatuses.Approved);
        var service = new CourseAccessService(context);

        Assert.IsFalse(await service.HasApprovedAccessAsync("user-1", 10));
    }

    // QA: Qué probamos: pedido rechazado.
    // QA: Esperado: no concede acceso aprobado ni se considera pendiente.
    [TestMethod]
    public async Task PedidoRechazado_NoConcedeAccesoNiBloqueaCompra()
    {
        await using var context = TestHelpers.CreateContext();
        await SeedOrderAsync(context, "user-1", 10, OrderStatuses.Rejected);
        var service = new CourseAccessService(context);

        Assert.IsFalse(await service.HasApprovedAccessAsync("user-1", 10));
        Assert.IsFalse(await service.HasPendingOrderAsync("user-1", 10));
    }

    // QA: Qué probamos: pedido pendiente para el mismo usuario y curso.
    // QA: Esperado: HasPendingOrderAsync devuelve true.
    [TestMethod]
    public async Task HasPendingOrderAsync_PedidoPendiente_DevuelveTrue()
    {
        await using var context = TestHelpers.CreateContext();
        await SeedOrderAsync(context, "user-1", 10, OrderStatuses.Pending);
        var service = new CourseAccessService(context);

        Assert.IsTrue(await service.HasPendingOrderAsync("user-1", 10));
    }

    // QA: Qué probamos: conjunto de cursos no disponibles con aprobados, pendientes, rechazados y duplicados.
    // QA: Esperado: devuelve IDs distintos de aprobados/pendientes del usuario y excluye rechazados/otros usuarios.
    [TestMethod]
    public async Task GetUnavailableCourseIdsAsync_FiltraEstadosUsuariosYDuplicados()
    {
        await using var context = TestHelpers.CreateContext();
        await SeedOrderAsync(context, "user-1", 10, OrderStatuses.Approved);
        await SeedOrderAsync(context, "user-1", 20, OrderStatuses.Pending);
        await SeedOrderAsync(context, "user-1", 30, OrderStatuses.Rejected);
        await SeedOrderAsync(context, "user-2", 40, OrderStatuses.Approved);
        await SeedOrderAsync(context, "user-1", 10, OrderStatuses.Pending);

        var result = await new CourseAccessService(context).GetUnavailableCourseIdsAsync("user-1");

        CollectionAssert.AreEquivalent(new[] { 10, 20 }, result.ToArray());
    }

    // QA: Qué probamos: usuario sin historial de pedidos.
    // QA: Esperado: devuelve conjunto vacío y no lanza errores.
    [TestMethod]
    public async Task GetUnavailableCourseIdsAsync_SinPedidos_DevuelveVacio()
    {
        await using var context = TestHelpers.CreateContext();
        var result = await new CourseAccessService(context).GetUnavailableCourseIdsAsync("sin-pedidos");

        Assert.AreEqual(0, result.Count);
    }

    private static async Task SeedOrderAsync(
        ATRIUM.Infrastructure.Context.AtriumDbContext context,
        string userId,
        int courseId,
        string status)
    {
        var order = new Pedido
        {
            UserId = userId,
            EstadoPedido = status,
            FormaPago = PaymentMethods.Yape,
            TotalPedido = 100m,
            Direccion = "Dirección",
            Provincia = "Cajamarca",
            Localidad = "Cajamarca",
            Telefono = "999999999",
            Detalles =
            [
                new PedidoDetalle
                {
                    IdCurso = courseId,
                    Cantidad = 1,
                    PrecioIndividual = 100m
                }
            ]
        };

        context.Pedidos.Add(order);
        await context.SaveChangesAsync();
    }
}
