namespace JobTrackerService.Shared;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) => obj is ValueObject other &&
        GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public override int GetHashCode() => GetEqualityComponents()
        .Aggregate(0, (hash, value) => HashCode.Combine(hash, value));
}

public abstract class AggregateRoot<TId>
{
    private readonly List<IDomainEvent> domainEvents = [];
    public TId Id { get; protected init; } = default!;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => domainEvents.Clear();
}

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}

public sealed record Result<T>(bool IsSuccess, T? Value, string? Error)
{
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}

public sealed record PagedList<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);