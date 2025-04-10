using FluentValidation;

namespace FloByte.Application.Features.Workflows.Commands;

public class ConnectWorkflowNodesValidator : AbstractValidator<ConnectWorkflowNodes>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowEngine _workflowEngine;

    public ConnectWorkflowNodesValidator(
        IApplicationDbContext context,
        IWorkflowEngine workflowEngine)
    {
        _context = context;
        _workflowEngine = workflowEngine;

        RuleFor(x => x.WorkflowId)
            .NotEmpty()
            .MustAsync(WorkflowExists)
            .WithMessage("Workflow does not exist");

        RuleFor(x => x.SourceNodeId)
            .NotEmpty()
            .MustAsync(NodeExists)
            .WithMessage("Source node does not exist")
            .Must((command, sourceId) => sourceId != command.TargetNodeId)
            .WithMessage("Source and target nodes cannot be the same");

        RuleFor(x => x.TargetNodeId)
            .NotEmpty()
            .MustAsync(NodeExists)
            .WithMessage("Target node does not exist");

        RuleFor(x => x)
            .MustAsync(NodesInSameWorkflow)
            .WithMessage("Nodes must belong to the same workflow")
            .MustAsync(ConnectionNotExists)
            .WithMessage("Connection already exists between these nodes")
            .MustAsync(NoCircularDependency)
            .WithMessage("Connection would create a circular dependency");

        RuleFor(x => x.Label)
            .MaximumLength(50)
            .When(x => x.Label != null);
    }

    private async Task<bool> WorkflowExists(Guid workflowId, CancellationToken ct)
    {
        return await _context.Workflows
            .AnyAsync(w => w.Id == workflowId, ct);
    }

    private async Task<bool> NodeExists(Guid nodeId, CancellationToken ct)
    {
        return await _context.WorkflowNodes
            .AnyAsync(n => n.Id == nodeId, ct);
    }

    private async Task<bool> NodesInSameWorkflow(ConnectWorkflowNodes command, CancellationToken ct)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Nodes)
            .FirstOrDefaultAsync(w => w.Id == command.WorkflowId, ct);

        if (workflow == null)
            return false;

        return workflow.Nodes.Any(n => n.Id == command.SourceNodeId)
            && workflow.Nodes.Any(n => n.Id == command.TargetNodeId);
    }

    private async Task<bool> ConnectionNotExists(ConnectWorkflowNodes command, CancellationToken ct)
    {
        return !await _context.WorkflowConnections
            .AnyAsync(c => 
                c.SourceNodeId == command.SourceNodeId && 
                c.TargetNodeId == command.TargetNodeId, ct);
    }

    private async Task<bool> NoCircularDependency(ConnectWorkflowNodes command, CancellationToken ct)
    {
        return await _workflowEngine.ValidateConnectionAsync(
            command.WorkflowId,
            command.SourceNodeId,
            command.TargetNodeId,
            ct);
    }
}
