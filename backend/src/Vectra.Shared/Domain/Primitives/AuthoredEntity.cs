namespace Vectra.Shared.Domain.Primitives
{
    /// <summary>
    /// Сущность с полным отслеживанием авторства
    /// </summary>
    public abstract class AuthoredEntity : AuditableEntity
    {
        public Guid CreatedBy { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }

        protected AuthoredEntity()
        {
        }

        protected AuthoredEntity(Guid createdBy) : base()
        {
            CreatedBy = createdBy;
        }
        public void UpdateAuthor(Guid updatedBy)
        {
            UpdatedBy = updatedBy;
            UpdateTimestamp();
        }
    }
}

