using JobTrackerService.Jobs.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerService.Jobs.Infrastructure;

public sealed partial class JobRepository(JobsDbContext db) : IJobRepository
{
    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.Jobs.Include(job => job.Photos).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default) =>
        await db.Jobs.AddAsync(job, cancellationToken);

}