using JobTrackerService.Jobs.Application;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackerService.Api.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateJobCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new StartJobCommand(id), cancellationToken));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new CancelJobCommand(id), cancellationToken));
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new CompleteJobCommand(id), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchJobsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(query, cancellationToken));
    }
}