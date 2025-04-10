using FloByte.Domain.Common;

namespace FloByte.Domain.Entities;

public class WorkflowConnection : BaseEntity
{
    public WorkflowNode SourceNode { get; private set; }
    public WorkflowNode TargetNode { get; private set; }
    public string Label { get; private set; }
    public Dictionary<string, object> Conditions { get; private set; }

    private WorkflowConnection() { } // For EF Core

    public WorkflowConnection(WorkflowNode sourceNode, WorkflowNode targetNode, string label = "")
    {
        Id = Guid.NewGuid();
        SourceNode = sourceNode;
        TargetNode = targetNode;
        Label = label;
        Conditions = new Dictionary<string, object>();
    }

    public void AddCondition(string key, object value)
    {
        Conditions[key] = value;
    }

    public void UpdateLabel(string newLabel)
    {
        Label = newLabel;
    }
}
