using JobTrackerService.Jobs.Domain;
using JobTrackerService.Shared;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerService.Jobs.Infrastructure;

public sealed partial class JobRepository
{
    public async Task<PagedList<Job>> SearchAsync(JobStatus? status, DateTime? from, DateTime? to, Guid? assigneeId,
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = db.Jobs.AsNoTracking().AsQueryable();
        if (status is not null) query = query.Where(x => x.Status == status);
        if (from is not null) query = query.Where(x => x.ScheduledDate >= from);
        if (to is not null) query = query.Where(x => x.ScheduledDate <= to);
        if (assigneeId is not null) query = query.Where(x => x.AssigneeId == assigneeId);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(x => x.ScheduledDate).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedList<Job>(items, page, pageSize, total);
    }
}