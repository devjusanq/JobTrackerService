using JobTrackerService.Shared;

namespace JobTrackerService.Jobs.Domain;

public sealed record JobCreatedDomainEvent(Guid JobId, Guid? AssigneeId, DateTime OccurredOn) : IDomainEvent;
public sealed record JobCompletedDomainEvent(Guid JobId, Guid CustomerId, DateTime CompletedAt, DateTime OccurredOn) : IDomainEvent;
public sealed record JobCancelledDomainEvent(Guid JobId, DateTime OccurredOn) : IDomainEvent;

public enum JobStatus { Draft, Scheduled, InProgress, Completed, Cancelled }