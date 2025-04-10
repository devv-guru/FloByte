using FluentValidation;

namespace FloByte.Application.Features.Workflows.Commands;

public class AddWorkflowNodeValidator : AbstractValidator<AddWorkflowNode>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowEngine _workflowEngine;

    public AddWorkflowNodeValidator(
        IApplicationDbContext context,
        IWorkflowEngine workflowEngine)
    {
        _context = context;
        _workflowEngine = workflowEngine;

        RuleFor(x => x.WorkflowId)
            .NotEmpty()
            .MustAsync(WorkflowExists)
            .WithMessage("Workflow does not exist");

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(BeSupportedNodeType)
            .WithMessage("Unsupported node type");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-zA-Z0-9\-_\s]+$")
            .WithMessage("Node name can only contain letters, numbers, spaces, hyphens, and underscores");

        RuleFor(x => x.X)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(10000);

        RuleFor(x => x.Y)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(10000);
    }

    private async Task<bool> WorkflowExists(Guid workflowId, CancellationToken ct)
    {
        return await _context.Workflows
            .AnyAsync(w => w.Id == workflowId, ct);
    }

    private async Task<bool> BeSupportedNodeType(string type, CancellationToken ct)
    {
        var supportedTypes = await _workflowEngine.GetSupportedNodeTypesAsync(ct);
        return supportedTypes.Contains(type);
    }
}
