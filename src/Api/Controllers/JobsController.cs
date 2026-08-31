using JobTrackerService.Jobs.Application;
using JobTrackerService.Jobs.Domain;
using JobTrackerService.Jobs.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTrackerService.Api.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController(IMediator mediator, IJobRepository jobs, JobsDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateJobCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }

    [HttpPost("{id:guid}/photos")]
    public async Task<IActionResult> UploadPhotos(Guid id, IFormFileCollection files, CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            return BadRequest(new { error = "At least one image file is required." });
        }

        var job = await jobs.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return NotFound(new { error = "Job not found." });
        }

        var uploadsRoot = Path.Combine(environment.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot"), "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var uploadedUrls = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0 || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var safeFileName = Path.GetFileName(file.FileName);
            var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
            var targetPath = Path.Combine(uploadsRoot, uniqueName);

            await using (var stream = System.IO.File.Create(targetPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relativeUrl = $"/uploads/{uniqueName}";
            var photoResult = job.AddPhoto(relativeUrl, DateTime.UtcNow, null);
            if (!photoResult.IsSuccess || photoResult.Value is null)
            {
                continue;
            }

            db.JobPhotos.Add(photoResult.Value);
            db.Entry(photoResult.Value).Property<Guid>("JobId").CurrentValue = job.Id;
            uploadedUrls.Add(relativeUrl);
        }

        if (uploadedUrls.Count == 0)
        {
            return BadRequest(new { error = "No valid image files were provided." });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { photos = uploadedUrls });
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