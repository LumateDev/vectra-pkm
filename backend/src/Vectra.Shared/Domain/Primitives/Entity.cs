namespace Vectra.Shared.Domain.Primitives
{
    /// <summary>
    /// Базовая сущность только с Id
    /// </summary>
    public abstract class Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public Guid Id { get; protected set; }
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected Entity()
        {
            Id = Guid.NewGuid();
        }

        protected Entity(Guid id)
        {
            Id = id;
        }

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
 
    }
}

