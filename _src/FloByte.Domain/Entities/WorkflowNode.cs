using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class WorkflowNode : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty;
    public Dictionary<string, object> Properties { get; private set; }
    public Position Position { get; private set; }

    private WorkflowNode() { } // For EF Core

    public WorkflowNode(string name, string type, Position position)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        Position = position;
        Properties = new Dictionary<string, object>();
    }

    public void UpdatePosition(Position newPosition)
    {
        Position = newPosition;
    }

    public void UpdateProperty(string key, object value)
    {
        Properties[key] = value;
    }
}

public record Position(double X, double Y);
