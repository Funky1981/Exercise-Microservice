using Exercise.Application.Features.ExerciseLogs.Commands.AddExerciseLogEntry;
using Exercise.Application.Features.ExerciseLogs.Commands.CompleteExerciseLog;
using Exercise.Application.Features.ExerciseLogs.Commands.CreateExerciseLog;
using Exercise.Application.Features.ExerciseLogs.Commands.DeleteExerciseLog;
using Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogById;
using Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogsByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exercise.API
{
    public static class MapExerciseLogEndpoints
    {
        public static void MapExerciseLogEndpointsRoute(this WebApplication app)
        {
            var group = app.MapGroup("/api/exercise-logs")
                           .WithTags("ExerciseLogs")
                           .WithOpenApi()
                           .RequireAuthorization();

            // GET /api/exercise-logs?userId=&pageNumber=&pageSize=
            group.MapGet("/",
                async ([FromQuery] Guid userId, [FromQuery] int pageNumber, [FromQuery] int pageSize,
                       IMediator mediator, CancellationToken ct) =>
                {
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
            .WithSummary("Get a paged list of exercise logs for a user")
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
            group.MapPost("/", async (CreateExerciseLogCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetExerciseLogById", new { id }, new { id });
            })
            .WithName("CreateExerciseLog")
            .WithSummary("Create a new exercise log")
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
