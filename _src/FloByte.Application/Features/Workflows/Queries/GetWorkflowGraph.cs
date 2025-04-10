using FloByte.Application.Common.Errors;
using FloByte.Domain.Entities;
using FluentResults;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FloByte.Application.Features.Workflows.Queries;

public record GetWorkflowGraph(Guid WorkflowId) : IQuery<Result<WorkflowGraph>>;

public record WorkflowGraph(
    Guid Id,
    string Name,
    string Description,
    List<WorkflowNode> Nodes,
    List<WorkflowConnection> Connections);

public class GetWorkflowGraphHandler : IQueryHandler<GetWorkflowGraph, Result<WorkflowGraph>>
{
    private readonly IApplicationDbContext _context;

    public GetWorkflowGraphHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async ValueTask<Result<WorkflowGraph>> Handle(GetWorkflowGraph query, CancellationToken ct)
    {
        var workflow = await _context.Workflows
            .Include(w => w.Nodes)
            .Include(w => w.Connections)
            .FirstOrDefaultAsync(w => w.Id == query.WorkflowId, ct);

        if (workflow is null)
            return Result.Fail(new NotFoundError($"Workflow with ID {query.WorkflowId} not found"));

        var graph = new WorkflowGraph(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Nodes.ToList(),
            workflow.Connections.ToList());

        return Result.Ok(graph);
    }
}
