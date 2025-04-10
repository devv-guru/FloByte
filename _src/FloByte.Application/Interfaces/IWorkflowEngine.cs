using FloByte.Domain.Entities;
using FluentResults;

namespace FloByte.Application.Interfaces;

public interface IWorkflowEngine
{
    Task<Result<Workflow>> CreateWorkflowAsync(string name, string description);
    Task<Result> ValidateWorkflowAsync(Workflow workflow);
    Task<Result> ExecuteWorkflowAsync(Workflow workflow, Dictionary<string, object> parameters);
    Task<Result<WorkflowNode>> AddNodeAsync(Workflow workflow, string type, string name, Position position);
    Task<Result<WorkflowConnection>> ConnectNodesAsync(WorkflowNode source, WorkflowNode target, string? label = null);
}
