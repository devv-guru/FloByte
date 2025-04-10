using FloByte.Domain.Common;

namespace FloByte.Domain.ValueObjects;

public class OidcClaims : ValueObject
{
    public string SubjectId { get; }
    public string Provider { get; }
    public string Email { get; }
    public string Name { get; }
    public Dictionary<string, string> AdditionalClaims { get; }

    public OidcClaims(string subjectId, string provider, string email, string name, Dictionary<string, string>? additionalClaims = null)
    {
        SubjectId = subjectId;
        Provider = provider;
        Email = email;
        Name = name;
        AdditionalClaims = additionalClaims ?? new Dictionary<string, string>();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return SubjectId;
        yield return Provider;
        yield return Email;
        yield return Name;
        foreach (var claim in AdditionalClaims.OrderBy(x => x.Key))
        {
            yield return $"{claim.Key}:{claim.Value}";
        }
    }
}
