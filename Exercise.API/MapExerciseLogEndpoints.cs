using Exercise.Application.Features.ExerciseLogs.Commands.AddExerciseLogEntry;
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
            var group = app.MapGroup("/api/exercise-logs")
                           .WithTags("ExerciseLogs")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api");

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
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // GET /api/exercise-logs/{id}
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetExerciseLogByIdQuery { Id = id }, ct);
                return result is null
                    ? Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"ExerciseLog with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    })
                    : Results.Ok(result);
            })
            .WithName("GetExerciseLogById")
            .WithSummary("Get a single exercise log by its ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

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
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/exercise-logs/{id}/entries
            group.MapPost("/{id:guid}/entries",
                async (Guid id, AddExerciseLogEntryCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    command.LogId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("AddExerciseLogEntry")
            .WithSummary("Add an exercise entry to a log")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/exercise-logs/{id}/complete
            group.MapPost("/{id:guid}/complete",
                async (Guid id, [FromBody] CompleteExerciseLogRequest? body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(
                        new CompleteExerciseLogCommand { LogId = id, TotalDuration = body?.TotalDuration }, ct);
                    return Results.NoContent();
                })
            .WithName("CompleteExerciseLog")
            .WithSummary("Mark an exercise log as completed")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // DELETE /api/exercise-logs/{id}
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteExerciseLogCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteExerciseLog")
            .WithSummary("Delete an exercise log by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }

    public record CompleteExerciseLogRequest(TimeSpan? TotalDuration);
}
