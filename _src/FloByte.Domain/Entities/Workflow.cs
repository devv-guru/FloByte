using FloByte.Domain.Common;
using FloByte.Domain.Events;

namespace FloByte.Domain.Entities;

public class Workflow : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public WorkflowStatus Status { get; private set; }
    private readonly List<WorkflowNode> _nodes = new();
    private readonly List<WorkflowConnection> _connections = new();
    
    public IReadOnlyCollection<WorkflowNode> Nodes => _nodes.AsReadOnly();
    public IReadOnlyCollection<WorkflowConnection> Connections => _connections.AsReadOnly();

    private Workflow() { } // For EF Core

    public Workflow(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Status = WorkflowStatus.Draft;
    }

    public void AddNode(WorkflowNode node)
    {
        _nodes.Add(node);
    }

    public void AddConnection(WorkflowConnection connection)
    {
        if (!_nodes.Contains(connection.SourceNode) || !_nodes.Contains(connection.TargetNode))
        {
            throw new InvalidOperationException("Connection nodes must be part of the workflow");
        }
        _connections.Add(connection);
    }

    public void Activate()
    {
        if (Status != WorkflowStatus.Draft)
        {
            throw new InvalidOperationException("Can only activate workflows in draft status");
        }
        Status = WorkflowStatus.Active;
    }

    public void Deactivate()
    {
        if (Status != WorkflowStatus.Active)
        {
            throw new InvalidOperationException("Can only deactivate active workflows");
        }
        Status = WorkflowStatus.Inactive;
    }
}

public enum WorkflowStatus
{
    Draft,
    Active,
    Inactive
}
