using FloByte.Domain.Common;
using FloByte.Domain.Enums;

namespace FloByte.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string SubjectId { get; private set; } = string.Empty;  // OIDC subject identifier
    public string Provider { get; private set; } = string.Empty;   // Identity provider (e.g., "Microsoft", "Google")
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public UserProfile Profile { get; private set; }
    private readonly List<Project> _projects = new();
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    private User() { } // For EF Core

    public User(string email, string username, string subjectId, string provider)
    {
        Id = Guid.NewGuid();
        Email = email;
        Username = username;
        SubjectId = subjectId;
        Provider = provider;
        Role = UserRole.User;
        IsActive = true;
        Profile = new UserProfile(this);
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void AddProject(Project project)
    {
        if (!_projects.Contains(project))
        {
            _projects.Add(project);
        }
    }
}
