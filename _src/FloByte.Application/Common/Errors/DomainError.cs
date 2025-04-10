using FluentResults;

namespace FloByte.Application.Common.Errors;

public class DomainError : Error
{
    public DomainError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "Domain");
    }
}

public class ValidationError : Error
{
    public ValidationError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "Validation");
    }
}

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "NotFound");
    }
}

public class AuthorizationError : Error
{
    public AuthorizationError(string message) : base(message)
    {
        Metadata.Add("ErrorType", "Authorization");
    }
}
