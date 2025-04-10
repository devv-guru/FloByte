using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Workflows.Commands;

public record ConnectWorkflowNodes(
    Guid WorkflowId,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label = null) : ICommand<Result<WorkflowConnection>>;

public class ConnectWorkflowNodesHandler : ICommandHandler<ConnectWorkflowNodes, Result<WorkflowConnection>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowEngine _workflowEngine;

    public ConnectWorkflowNodesHandler(
        IApplicationDbContext context,
        IWorkflowEngine workflowEngine)
    {
        _context = context;
        _workflowEngine = workflowEngine;
    }

    public async ValueTask<Result<WorkflowConnection>> Handle(ConnectWorkflowNodes command, CancellationToken ct)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Nodes)
            .FirstOrDefaultAsync(w => w.Id == command.WorkflowId, ct);

        if (workflow is null)
            return Result.Fail(new NotFoundError($"Workflow with ID {command.WorkflowId} not found"));

        var sourceNode = workflow.Nodes.FirstOrDefault(n => n.Id == command.SourceNodeId);
        if (sourceNode is null)
            return Result.Fail(new NotFoundError($"Source node with ID {command.SourceNodeId} not found"));

        var targetNode = workflow.Nodes.FirstOrDefault(n => n.Id == command.TargetNodeId);
        if (targetNode is null)
            return Result.Fail(new NotFoundError($"Target node with ID {command.TargetNodeId} not found"));

        var connectionResult = await _workflowEngine.ConnectNodesAsync(
            sourceNode,
            targetNode,
            command.Label);

        if (connectionResult.IsFailed)
            return connectionResult;

        await _context.SaveChangesAsync(ct);
        return Result.Ok(connectionResult.Value);
    }
}
