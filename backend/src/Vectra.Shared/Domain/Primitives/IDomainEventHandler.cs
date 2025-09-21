using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vectra.Shared.Domain.Primitives
{
    public interface IDomainEventHandler<in T> where T : IDomainEvent
    {
        Task HandleAsync(T domainEvent, CancellationToken cancellationToken = default);
    }
}
