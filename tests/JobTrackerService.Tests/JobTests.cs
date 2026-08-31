using JobTrackerService.Jobs.Application;
using JobTrackerService.Jobs.Domain;
using Moq;
using Xunit;

namespace JobTrackerService.Tests;

public class JobTests
{
    [Fact]
    public void Create_WhenScheduledInPast_ReturnsFailure()
    {
        var address = new Address("Main St", "Madrid", "Madrid", "28001");

        var result = Job.Create(
            title: "Repair AC",
            description: "Fix the central unit",
            address: address,
            scheduledDate: DateTime.UtcNow.AddDays(-1),
            customerId: Guid.NewGuid(),
            organizationId: Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("A job cannot be scheduled in the past.", result.Error);
    }

    [Fact]
    public void TransitionTo_WhenScheduledToInProgress_IsSuccessful()
    {
        var job = CreateFutureJob();

        var result = job.TransitionTo(JobStatus.InProgress);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.InProgress, job.Status);
    }

    [Fact]
    public void TransitionTo_WhenInProgressToCompleted_IsSuccessful()
    {
        var job = CreateFutureJob();
        job.TransitionTo(JobStatus.InProgress);

        var result = job.TransitionTo(JobStatus.Completed);

        Assert.True(result.IsSuccess);
        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public void TransitionTo_WhenJobIsTerminal_Fails()
    {
        var job = CreateFutureJob();
        job.TransitionTo(JobStatus.InProgress);
        job.TransitionTo(JobStatus.Completed);

        var result = job.TransitionTo(JobStatus.Cancelled);

        Assert.False(result.IsSuccess);
        Assert.Equal("Terminal jobs cannot transition.", result.Error);
    }

    [Fact]
    public async Task Handle_WhenCreateJobCommandIsProcessed_AddsJobAndRaisesDomainEvent()
    {
        var address = new Address("Main St", "Madrid", "Madrid", "28001");
        var command = new CreateJobCommand(
            Title: "Repair AC",
            Description: "Fix the central unit",
            Address: address,
            ScheduledDate: DateTime.UtcNow.AddDays(2),
            CustomerId: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            AssigneeId: Guid.NewGuid());

        var repo = new Mock<IJobRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Job? addedJob = null;
        repo.Setup(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .Callback<Job, CancellationToken>((job, _) => addedJob = job)
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateJobCommandHandler(repo.Object, unitOfWork.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(addedJob);
        Assert.Contains(addedJob.DomainEvents, e => e is JobCreatedDomainEvent);
        repo.Verify(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Address_WhenSameValuesCreated_AreStructurallyEqual()
    {
        var left = new Address("Main St", "Madrid", "Madrid", "28001", 40.42m, -3.7m);
        var right = new Address("Main St", "Madrid", "Madrid", "28001", 40.42m, -3.7m);

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Address_WhenValuesDiffer_AreNotEqual()
    {
        var left = new Address("Main St", "Madrid", "Madrid", "28001");
        var right = new Address("Second St", "Madrid", "Madrid", "28001");

        Assert.NotEqual(left, right);
    }

    private static Job CreateFutureJob()
    {
        var result = Job.Create(
            title: "Repair AC",
            description: "Fix the central unit",
            address: new Address("Main St", "Madrid", "Madrid", "28001"),
            scheduledDate: DateTime.UtcNow.AddDays(2),
            customerId: Guid.NewGuid(),
            organizationId: Guid.NewGuid());

        Assert.True(result.IsSuccess);
        return result.Value!;
    }
}
