using FloByte.Domain.Common;
using FloByte.Domain.Enums;

namespace FloByte.Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ProjectStatus Status { get; private set; }
    private readonly List<User> _members = new();
    private readonly List<Workflow> _workflows = new();
    private readonly List<CodeFile> _codeFiles = new();
    
    public IReadOnlyCollection<User> Members => _members.AsReadOnly();
    public IReadOnlyCollection<Workflow> Workflows => _workflows.AsReadOnly();
    public IReadOnlyCollection<CodeFile> CodeFiles => _codeFiles.AsReadOnly();

    private Project() { } // For EF Core

    public Project(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Status = ProjectStatus.Planning;
    }

    public void AddMember(User user)
    {
        if (!_members.Contains(user))
        {
            _members.Add(user);
            user.AddProject(this);
        }
    }

    public void AddWorkflow(Workflow workflow)
    {
        if (!_workflows.Contains(workflow))
        {
            _workflows.Add(workflow);
        }
    }

    public void AddCodeFile(CodeFile codeFile)
    {
        if (!_codeFiles.Contains(codeFile))
        {
            _codeFiles.Add(codeFile);
        }
    }

    public void UpdateStatus(ProjectStatus newStatus)
    {
        Status = newStatus;
    }

    public void UpdateDetails(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
