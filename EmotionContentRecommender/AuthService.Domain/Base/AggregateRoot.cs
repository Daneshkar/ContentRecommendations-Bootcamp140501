namespace AuthService.Domain.Base;


public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public long Id { get; protected set; }

    protected AggregateRoot() { }

    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
        => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
        => _domainEvents.Clear();

    protected void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
    public override bool Equals(object? obj)
    {
        if (obj is not AggregateRoot other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        if (Id == 0 || other.Id == 0) return false;
        return Id == other.Id;
    }

    public override int GetHashCode()
        => (GetType().Name + Id).GetHashCode();

    public static bool operator ==(AggregateRoot? left, AggregateRoot? right)
        => left is null && right is null || left is not null && left.Equals(right);

    public static bool operator !=(AggregateRoot? left, AggregateRoot? right)
        => !(left == right);
}
