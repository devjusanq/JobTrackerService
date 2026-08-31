using JobTrackerService.Jobs.Domain;
using JobTrackerService.Shared;
using MediatR;

namespace JobTrackerService.Jobs.Application;

public sealed record SearchJobsQuery(JobStatus? Status, DateTime? From, DateTime? To, Guid? AssigneeId,
    string? Search, int Page = 1, int PageSize = 25) : IRequest<Result<PagedList<JobResponse>>>;

public sealed record JobResponse(Guid Id, string Title, string Description, JobStatus Status, DateTime ScheduledDate,
    Guid? AssigneeId, Guid CustomerId, Guid OrganizationId, Address Address);

internal sealed class SearchJobsQueryHandler(IJobRepository jobs) : IRequestHandler<SearchJobsQuery, Result<PagedList<JobResponse>>>
{
    private static DateTime? NormalizeUtc(DateTime? value)
    {
        if (value is not DateTime date) return null;
        return date.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc),
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => date
        };
    }

    public async Task<Result<PagedList<JobResponse>>> Handle(SearchJobsQuery request, CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize is < 1 or > 100) return Result<PagedList<JobResponse>>.Failure("Invalid pagination.");

        var from = NormalizeUtc(request.From);
        var to = NormalizeUtc(request.To);

        var result = await jobs.SearchAsync(request.Status, from, to, request.AssigneeId, request.Search,
            request.Page, request.PageSize, cancellationToken);
        var response = new PagedList<JobResponse>(result.Items.Select(job => new JobResponse(job.Id, job.Title, job.Description,
            job.Status, job.ScheduledDate, job.AssigneeId, job.CustomerId, job.OrganizationId, job.Address)).ToList(),
            result.Page, result.PageSize, result.TotalCount);
        return Result<PagedList<JobResponse>>.Success(response);
    }
}