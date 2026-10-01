namespace ATRIUM.Web.Services;

public interface ICourseAccessService
{
    Task<bool> HasApprovedAccessAsync(string userId, int courseId, CancellationToken cancellationToken = default);
    Task<bool> HasPendingOrderAsync(string userId, int courseId, CancellationToken cancellationToken = default);
    Task<string?> GetBlockingOrderStatusAsync(string userId, int courseId, CancellationToken cancellationToken = default);
    Task<HashSet<int>> GetUnavailableCourseIdsAsync(string userId, CancellationToken cancellationToken = default);
}
