using Asp.Versioning;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.ExerciseLogs.Commands.AddExerciseLogEntry;
using Exercise.Application.Features.ExerciseLogs.Dtos;
using Exercise.Application.Features.ExerciseLogs.Commands.CompleteExerciseLog;
using Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog;
using Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog;
using Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogById;
using Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogsByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exercise.API
{
    public static class MapExerciseLogEndpoints
    {
        public static void MapExerciseLogEndpointsRoute(this WebApplication app)
        {
            var versionSet = app.NewApiVersionSet()
                               .HasApiVersion(new ApiVersion(1, 0))
                               .ReportApiVersions()
                               .Build();

            var group = app.MapGroup("/api/exercise-logs")
                           .WithTags("ExerciseLogs")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // GET /api/exercise-logs?pageNumber=&pageSize=
            // userId is derived from the JWT sub claim — users can only see their own logs
            group.MapGet("/",
                async (ClaimsPrincipal user, [FromQuery] int pageNumber, [FromQuery] int pageSize,
                       IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId))
                        return Results.Unauthorized();

                    var result = await mediator.Send(
                        new GetExerciseLogsByUserIdQuery
                        {
                            UserId = userId,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetExerciseLogsByUserId")
            .WithSummary("Get a paged list of the authenticated user's exercise logs")
            .WithDescription("pageNumber and pageSize are required. Results are scoped to the authenticated user's JWT — users can only see their own logs.")
            .Produces<PagedResult<ExerciseLogDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // GET /api/exercise-logs/{id}
            group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
            {
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var userId))
                    return Results.Unauthorized();

                var result = await mediator.Send(new GetExerciseLogByIdQuery { Id = id }, ct);
                if (result is null)
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"ExerciseLog with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    });

                if (result.UserId != userId)
                    return Results.Forbid();

                return Results.Ok(result);
            })
            .WithName("GetExerciseLogById")
            .WithSummary("Get a single exercise log by its ID")
            .Produces<ExerciseLogDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/exercise-logs
            // userId is set from the JWT claim — the request body UserId field is ignored
            group.MapPost("/", async (ClaimsPrincipal user, CreateExerciseLogCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var userId))
                    return Results.Unauthorized();

                command.UserId = userId;
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetExerciseLogById", new { id }, new { id });
            })
            .WithName("CreateExerciseLog")
            .WithSummary("Create a new exercise log for the authenticated user")
            .WithDescription("The userId is extracted from the JWT claim and applied automatically — any UserId in the request body is ignored.")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/exercise-logs/{id}/entries
            group.MapPost("/{id:guid}/entries",
                async (Guid id, ClaimsPrincipal user, AddExerciseLogEntryCommand command,
                       IExerciseLogRepository logRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var log = await logRepo.GetByIdAsync(id, ct);
                    if (log is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"ExerciseLog with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (log.UserId != userId) return Results.Forbid();

                    command.LogId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("AddExerciseLogEntry")
            .WithSummary("Add an exercise entry to a log")
            .WithDescription("Sets the sets, reps, and optional duration for a specific exercise within a log session.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/exercise-logs/{id}/complete
            group.MapPost("/{id:guid}/complete",
                async (Guid id, ClaimsPrincipal user, [FromBody] CompleteExerciseLogRequest? body,
                       IExerciseLogRepository logRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var log = await logRepo.GetByIdAsync(id, ct);
                    if (log is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"ExerciseLog with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (log.UserId != userId) return Results.Forbid();

                    await mediator.Send(
                        new CompleteExerciseLogCommand { LogId = id, TotalDuration = body?.TotalDuration }, ct);
                    return Results.NoContent();
                })
            .WithName("CompleteExerciseLog")
            .WithSummary("Mark an exercise log as completed")
            .WithDescription("Optionally supply TotalDuration as a TimeSpan string (e.g. \"00:45:00\"). If omitted, duration is left as null.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // DELETE /api/exercise-logs/{id}
            group.MapDelete("/{id:guid}",
                async (Guid id, ClaimsPrincipal user, IExerciseLogRepository logRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var log = await logRepo.GetByIdAsync(id, ct);
                    if (log is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"ExerciseLog with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (log.UserId != userId) return Results.Forbid();

                    await mediator.Send(new DeleteExerciseLogCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteExerciseLog")
            .WithSummary("Delete an exercise log by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
        }
    }

    public record CompleteExerciseLogRequest(TimeSpan? TotalDuration);
}
