namespace Lacuna.Space.Domain.Abstractions;

public abstract class Entity<T> where T : class
{
    public T Id { get; }

    protected Entity(T id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<T> other &&
               GetType() == other.GetType() &&
               Id.Equals(other.Id);
    }

    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => $"{GetType().Name}({Id})";
}