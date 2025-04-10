using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Workflows.Commands;

public record AddWorkflowNode(
    Guid WorkflowId,
    string Type,
    string Name,
    double X,
    double Y) : ICommand<Result<WorkflowNode>>;

public class AddWorkflowNodeHandler : ICommandHandler<AddWorkflowNode, Result<WorkflowNode>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowEngine _workflowEngine;

    public AddWorkflowNodeHandler(
        IApplicationDbContext context,
        IWorkflowEngine workflowEngine)
    {
        _context = context;
        _workflowEngine = workflowEngine;
    }

    public async ValueTask<Result<WorkflowNode>> Handle(AddWorkflowNode command, CancellationToken ct)
    {
        var workflow = await _context.Workflows
            .FirstOrDefaultAsync(w => w.Id == command.WorkflowId, ct);

        if (workflow is null)
            return Result.Fail(new NotFoundError($"Workflow with ID {command.WorkflowId} not found"));

        var position = new Position(command.X, command.Y);
        var nodeResult = await _workflowEngine.AddNodeAsync(
            workflow,
            command.Type,
            command.Name,
            position);

        if (nodeResult.IsFailed)
            return nodeResult;

        await _context.SaveChangesAsync(ct);
        return Result.Ok(nodeResult.Value);
    }
}
