using FluentValidation;
using JobTrackerService.Jobs.Domain;
using JobTrackerService.Shared;
using MediatR;

namespace JobTrackerService.Jobs.Application;

public sealed record CreateJobCommand(string Title, string Description, Address Address, DateTime ScheduledDate,
    Guid CustomerId, Guid OrganizationId, Guid? AssigneeId) : IRequest<Result<Guid>>;

internal sealed class CreateJobCommandValidator : AbstractValidator<CreateJobCommand>
{
    public CreateJobCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ScheduledDate).GreaterThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.CustomerId).NotEmpty(); RuleFor(x => x.OrganizationId).NotEmpty();
    }
}

internal sealed class CreateJobCommandHandler(IJobRepository jobs, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateJobCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        var result = Job.Create(request.Title, request.Description, request.Address, request.ScheduledDate,
            request.CustomerId, request.OrganizationId, request.AssigneeId);
        if (!result.IsSuccess) return Result<Guid>.Failure(result.Error!);
        await jobs.AddAsync(result.Value!, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(result.Value!.Id);
    }
}

public sealed record StartJobCommand(Guid JobId) : IRequest<Result<Unit>>;

internal sealed class StartJobCommandValidator : AbstractValidator<StartJobCommand>
{
    public StartJobCommandValidator() => RuleFor(x => x.JobId).NotEmpty();
}

internal sealed class StartJobCommandHandler(IJobRepository jobs, IUnitOfWork unitOfWork)
    : IRequestHandler<StartJobCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(StartJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobs.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null) return Result<Unit>.Failure("Job was not found.");
        var transition = job.TransitionTo(JobStatus.InProgress);
        if (!transition.IsSuccess) return Result<Unit>.Failure(transition.Error!);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}

public sealed record CancelJobCommand(Guid JobId) : IRequest<Result<Unit>>;

internal sealed class CancelJobCommandValidator : AbstractValidator<CancelJobCommand>
{
    public CancelJobCommandValidator() => RuleFor(x => x.JobId).NotEmpty();
}

internal sealed class CancelJobCommandHandler(IJobRepository jobs, IUnitOfWork unitOfWork)
    : IRequestHandler<CancelJobCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CancelJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobs.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null) return Result<Unit>.Failure("Job was not found.");
        var transition = job.TransitionTo(JobStatus.Cancelled);
        if (!transition.IsSuccess) return Result<Unit>.Failure(transition.Error!);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}

public sealed record CompleteJobCommand(Guid JobId) : IRequest<Result<Unit>>;

internal sealed class CompleteJobCommandValidator : AbstractValidator<CompleteJobCommand>
{
    public CompleteJobCommandValidator() => RuleFor(x => x.JobId).NotEmpty();
}

internal sealed class CompleteJobCommandHandler(IJobRepository jobs, IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteJobCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CompleteJobCommand request, CancellationToken cancellationToken)
    {
        var job = await jobs.GetByIdAsync(request.JobId, cancellationToken);
        if (job is null) return Result<Unit>.Failure("Job was not found.");
        var transition = job.TransitionTo(JobStatus.Completed);
        if (!transition.IsSuccess) return Result<Unit>.Failure(transition.Error!);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success(Unit.Value);
    }
}