using FloByte.Domain.Common;
using FloByte.Domain.Entities;

namespace FloByte.Domain.Events;

public class UserLoggedInEvent : DomainEvent
{
    public User User { get; }
    public string Provider { get; }
    public string IpAddress { get; }

    public UserLoggedInEvent(User user, string ipAddress)
    {
        User = user;
        Provider = user.Provider;
        IpAddress = ipAddress;
    }
}
