using ATRIUM.Domain.Constants;
using ATRIUM.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ATRIUM.Web.Services;

public sealed class CourseAccessService : ICourseAccessService
{
    private readonly AtriumDbContext _context;

    public CourseAccessService(AtriumDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasApprovedAccessAsync(
        string userId,
        int courseId,
        CancellationToken cancellationToken = default) =>
        _context.PedidoDetalles.AnyAsync(
            detail => detail.IdCurso == courseId
                      && detail.Pedido != null
                      && detail.Pedido.UserId == userId
                      && detail.Pedido.EstadoPedido == OrderStatuses.Approved,
            cancellationToken);

    public Task<bool> HasPendingOrderAsync(
        string userId,
        int courseId,
        CancellationToken cancellationToken = default) =>
        _context.PedidoDetalles.AnyAsync(
            detail => detail.IdCurso == courseId
                      && detail.Pedido != null
                      && detail.Pedido.UserId == userId
                      && detail.Pedido.EstadoPedido == OrderStatuses.Pending,
            cancellationToken);

    public Task<string?> GetBlockingOrderStatusAsync(
        string userId,
        int courseId,
        CancellationToken cancellationToken = default) =>
        _context.PedidoDetalles
            .AsNoTracking()
            .Where(detail => detail.IdCurso == courseId
                             && detail.Pedido != null
                             && detail.Pedido.UserId == userId
                             && (detail.Pedido.EstadoPedido == OrderStatuses.Approved
                                 || detail.Pedido.EstadoPedido == OrderStatuses.Pending))
            .OrderBy(detail => detail.Pedido!.EstadoPedido == OrderStatuses.Approved ? 0 : 1)
            .Select(detail => detail.Pedido!.EstadoPedido)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<HashSet<int>> GetUnavailableCourseIdsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var courseIds = await _context.PedidoDetalles
            .AsNoTracking()
            .Where(detail => detail.Pedido != null
                             && detail.Pedido.UserId == userId
                             && (detail.Pedido.EstadoPedido == OrderStatuses.Approved
                                 || detail.Pedido.EstadoPedido == OrderStatuses.Pending))
            .Select(detail => detail.IdCurso)
            .Distinct()
            .ToListAsync(cancellationToken);

        return courseIds.ToHashSet();
    }
}
