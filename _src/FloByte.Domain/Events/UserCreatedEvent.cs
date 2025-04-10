using FloByte.Domain.Common;
using FloByte.Domain.Entities;

namespace FloByte.Domain.Events;

public class UserCreatedEvent : DomainEvent
{
    public User User { get; }
    public string Provider { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
        Provider = user.Provider;
    }
}
