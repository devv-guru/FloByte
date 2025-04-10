using FloByte.Domain.Common;
using FloByte.Domain.Entities;

namespace FloByte.Domain.Events;

public class WorkflowCreatedEvent : DomainEvent
{
    public Workflow Workflow { get; }

    public WorkflowCreatedEvent(Workflow workflow)
    {
        Workflow = workflow;
    }
}
