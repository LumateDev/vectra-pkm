namespace Vectra.Shared.Domain.Primitives
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }
    }
}
