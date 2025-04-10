using FloByte.Domain.Common;

namespace FloByte.Application.Common.Interfaces;

public interface IDomainEventService
{
    Task PublishAsync(DomainEvent domainEvent);
}
