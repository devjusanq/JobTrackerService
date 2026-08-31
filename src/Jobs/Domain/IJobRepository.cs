using JobTrackerService.Shared;

namespace JobTrackerService.Jobs.Domain;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
    Task<PagedList<Job>> SearchAsync(JobStatus? status, DateTime? from, DateTime? to, Guid? assigneeId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
}