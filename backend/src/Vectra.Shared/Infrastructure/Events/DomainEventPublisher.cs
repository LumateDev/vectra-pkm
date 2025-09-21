using Microsoft.Extensions.DependencyInjection;
using Vectra.Shared.Domain.Primitives;

namespace Vectra.Shared.Infrastructure.Events
{
    public interface IDomainEventPublisher
    {
        Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    }

    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventPublisher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

            // ➜➜➜ Получаем **конкретный** обработчик
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is null) return;

            // ➜➜➜ Вызываем напрямую
            var method = handlerType.GetMethod("HandleAsync");
            await (Task)method!.Invoke(handler, new object[] { domainEvent, cancellationToken })!;
        }
    }
}