namespace Vectra.Shared.Domain.Primitives
{

    /// <summary>
    /// Сущность с отслеживанием времени создания/изменения
    /// </summary>
    public abstract class AuditableEntity : Entity
    {
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }

        protected AuditableEntity()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
