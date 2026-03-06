using Asp.Versioning;
using Exercise.Application.Features.Workouts.Commands.AddExerciseToWorkout;
using Exercise.Application.Features.Workouts.Commands.CompleteWorkout;
using Exercise.Application.Features.Workouts.Commands.CreateWorkout;
using Exercise.Application.Features.Workouts.Commands.DeleteWorkout;
using Exercise.Application.Features.Workouts.Commands.RemoveExerciseFromWorkout;
using Exercise.Application.Features.Workouts.Commands.UpdateWorkout;
using Exercise.Application.Features.Workouts.Queries.GetWorkoutById;
using Exercise.Application.Features.Workouts.Queries.GetWorkoutsByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exercise.API
{
    public static class MapWorkoutEndpoints
    {
        public static void MapWorkoutEndpointsRoute(this WebApplication app)
        {
            var versionSet = app.NewApiVersionSet()
                               .HasApiVersion(new ApiVersion(1, 0))
                               .ReportApiVersions()
                               .Build();

            var group = app.MapGroup("/api/workouts")
                           .WithTags("Workouts")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // GET /api/workouts?pageNumber=&pageSize=
            // userId is derived from the JWT sub claim — users can only see their own workouts
            group.MapGet("/",
                async (ClaimsPrincipal user, [FromQuery] int pageNumber, [FromQuery] int pageSize,
                       IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId))
                        return Results.Unauthorized();

                    var result = await mediator.Send(
                        new GetWorkoutsByUserIdQuery(userId, pageNumber, pageSize), ct);
                    return Results.Ok(result);
                })
            .WithName("GetWorkoutsByUserId")
            .WithSummary("Get a paged list of the authenticated user's workouts")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // GET /api/workouts/{id}
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetWorkoutByIdQuery(id), ct);
                return result is null
                    ? Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"Workout with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    })
                    : Results.Ok(result);
            })
            .WithName("GetWorkoutById")
            .WithSummary("Get a single workout by its ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/workouts
            // userId is set from the JWT claim — the request body UserId field is ignored
            group.MapPost("/", async (ClaimsPrincipal user, CreateWorkoutCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var userId))
                    return Results.Unauthorized();

                command.UserId = userId;
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetWorkoutById", new { id }, new { id });
            })
            .WithName("CreateWorkout")
            .WithSummary("Create a new workout session for the authenticated user")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // PUT /api/workouts/{id}
            group.MapPut("/{id:guid}",
                async (Guid id, UpdateWorkoutCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    command.WorkoutId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("UpdateWorkout")
            .WithSummary("Update a workout's name, date, and notes")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/workouts/{id}/complete
            group.MapPost("/{id:guid}/complete",
                async (Guid id, [FromBody] CompleteWorkoutRequest body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new CompleteWorkoutCommand(id, body.Duration), ct);
                    return Results.NoContent();
                })
            .WithName("CompleteWorkout")
            .WithSummary("Mark a workout as completed with its total duration")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // DELETE /api/workouts/{id}
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteWorkoutCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteWorkout")
            .WithSummary("Soft-delete a workout by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/workouts/{id}/exercises
            group.MapPost("/{id:guid}/exercises",
                async (Guid id, [FromBody] AddExerciseRequest body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new AddExerciseToWorkoutCommand(id, body.ExerciseId), ct);
                    return Results.NoContent();
                })
            .WithName("AddExerciseToWorkout")
            .WithSummary("Add an exercise to a workout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // DELETE /api/workouts/{id}/exercises/{exerciseId}
            group.MapDelete("/{id:guid}/exercises/{exerciseId:guid}",
                async (Guid id, Guid exerciseId, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new RemoveExerciseFromWorkoutCommand(id, exerciseId), ct);
                    return Results.NoContent();
                })
            .WithName("RemoveExerciseFromWorkout")
            .WithSummary("Remove an exercise from a workout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }

    // Small request body model — avoids collision with CompleteWorkoutCommand constructor arguments
    public record CompleteWorkoutRequest(TimeSpan Duration);

    public record AddExerciseRequest(Guid ExerciseId);
}
