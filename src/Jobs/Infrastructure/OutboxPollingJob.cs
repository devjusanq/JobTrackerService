namespace JobTrackerService.Jobs.Infrastructure;

public sealed class OutboxPollingJob(OutboxDispatcher dispatcher)
{
    public Task RunAsync() => dispatcher.DispatchAsync();
}