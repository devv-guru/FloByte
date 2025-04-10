using FloByte.Application.Common.Interfaces;

namespace FloByte.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}
