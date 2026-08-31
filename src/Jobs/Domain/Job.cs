using JobTrackerService.Shared;

namespace JobTrackerService.Jobs.Domain;

public sealed class Job : AggregateRoot<Guid>
{
    private readonly List<JobPhoto> photos = [];
    private Job() { }

    private Job(Guid id, string title, string description, Address address, DateTime scheduledDate,
        Guid customerId, Guid organizationId, Guid? assigneeId)
    {
        Id = id; Title = title.Trim(); Description = description.Trim(); Address = address;
        ScheduledDate = scheduledDate; CustomerId = customerId; OrganizationId = organizationId; AssigneeId = assigneeId;
        Status = JobStatus.Scheduled; CreatedAt = DateTime.UtcNow;
        Raise(new JobCreatedDomainEvent(Id, AssigneeId, CreatedAt));
    }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public JobStatus Status { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public Guid? AssigneeId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<JobPhoto> Photos => photos.AsReadOnly();

    public static Result<Job> Create(string title, string description, Address address, DateTime scheduledDate,
        Guid customerId, Guid organizationId, Guid? assigneeId = null)
    {
        if (scheduledDate < DateTime.UtcNow) return Result<Job>.Failure("A job cannot be scheduled in the past.");
        return Result<Job>.Success(new Job(Guid.NewGuid(), title, description, address, scheduledDate, customerId, organizationId, assigneeId));
    }

    public Result<bool> TransitionTo(JobStatus newStatus)
    {
        if (Status is JobStatus.Completed or JobStatus.Cancelled) return Result<bool>.Failure("Terminal jobs cannot transition.");
        if (newStatus == JobStatus.InProgress && Status != JobStatus.Scheduled) return Result<bool>.Failure("Only scheduled jobs can start.");
        Status = newStatus; UpdatedAt = DateTime.UtcNow;
        if (newStatus == JobStatus.Completed) Raise(new JobCompletedDomainEvent(Id, CustomerId, UpdatedAt.Value, UpdatedAt.Value));
        if (newStatus == JobStatus.Cancelled) Raise(new JobCancelledDomainEvent(Id, UpdatedAt.Value));
        return Result<bool>.Success(true);
    }

    public Result<JobPhoto> AddPhoto(string url, DateTime capturedAt, string? caption)
    {
        var photo = new JobPhoto(Guid.NewGuid(), url, capturedAt, caption);
        photos.Add(photo); UpdatedAt = DateTime.UtcNow;
        return Result<JobPhoto>.Success(photo);
    }
}