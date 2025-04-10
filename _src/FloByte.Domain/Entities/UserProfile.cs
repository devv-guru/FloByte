using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class UserProfile : BaseEntity
{
    public User User { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public Dictionary<string, string> Preferences { get; private set; }

    private UserProfile() { } // For EF Core

    public UserProfile(User user)
    {
        Id = Guid.NewGuid();
        User = user;
        Preferences = new Dictionary<string, string>();
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string? bio = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Bio = bio;
    }

    public void UpdateAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }

    public void SetPreference(string key, string value)
    {
        Preferences[key] = value;
    }
}
