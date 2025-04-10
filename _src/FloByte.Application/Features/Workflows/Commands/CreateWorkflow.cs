using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Workflows.Commands;

public record CreateWorkflow(Guid ProjectId, string Name, string Description) : ICommand<Result<Workflow>>;

public class CreateWorkflowHandler : ICommandHandler<CreateWorkflow, Result<Workflow>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWorkflowEngine _workflowEngine;

    public CreateWorkflowHandler(
        IApplicationDbContext context,
        IWorkflowEngine workflowEngine)
    {
        _context = context;
        _workflowEngine = workflowEngine;
    }

    public async ValueTask<Result<Workflow>> Handle(CreateWorkflow command, CancellationToken ct)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);

        if (project is null)
            return Result.Fail(new NotFoundError($"Project with ID {command.ProjectId} not found"));

        var workflowResult = await _workflowEngine.CreateWorkflowAsync(command.Name, command.Description);
        if (workflowResult.IsFailed)
            return workflowResult;

        var workflow = workflowResult.Value;
        project.AddWorkflow(workflow);
        await _context.SaveChangesAsync(ct);

        return Result.Ok(workflow);
    }
}
