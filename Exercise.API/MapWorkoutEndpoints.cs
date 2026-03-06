using Asp.Versioning;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.Workouts.Commands.AddExerciseToWorkout;
using Exercise.Application.Features.Workouts.Dtos;
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
            .WithDescription("pageNumber and pageSize are required. Results are scoped to the authenticated user's JWT — users can only see their own workouts.")
            .Produces<PagedResult<WorkoutDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // GET /api/workouts/{id}
            group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
            {
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var userId))
                    return Results.Unauthorized();

                var result = await mediator.Send(new GetWorkoutByIdQuery(id), ct);
                if (result is null)
                    return Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"Workout with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    });

                if (result.UserId != userId)
                    return Results.Forbid();

                return Results.Ok(result);
            })
            .WithName("GetWorkoutById")
            .WithSummary("Get a single workout by its ID")
            .Produces<WorkoutDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

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
            .WithDescription("The userId is extracted from the JWT claim and applied automatically — any UserId in the request body is ignored.")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // PUT /api/workouts/{id}
            group.MapPut("/{id:guid}",
                async (Guid id, ClaimsPrincipal user, UpdateWorkoutCommand command,
                       IWorkoutRepository workoutRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var workout = await workoutRepo.GetByIdAsync(id, ct);
                    if (workout is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"Workout with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (workout.UserId != userId) return Results.Forbid();

                    command.WorkoutId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("UpdateWorkout")
            .WithSummary("Update a workout's name, date, and notes")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/workouts/{id}/complete
            group.MapPost("/{id:guid}/complete",
                async (Guid id, ClaimsPrincipal user, [FromBody] CompleteWorkoutRequest body,
                       IWorkoutRepository workoutRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var workout = await workoutRepo.GetByIdAsync(id, ct);
                    if (workout is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"Workout with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (workout.UserId != userId) return Results.Forbid();

                    await mediator.Send(new CompleteWorkoutCommand(id, body.Duration), ct);
                    return Results.NoContent();
                })
            .WithName("CompleteWorkout")
            .WithSummary("Mark a workout as completed with its total duration")
            .WithDescription("Supply Duration as a TimeSpan string in the request body, e.g. \"00:45:00\" for 45 minutes.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // DELETE /api/workouts/{id}
            group.MapDelete("/{id:guid}",
                async (Guid id, ClaimsPrincipal user, IWorkoutRepository workoutRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var workout = await workoutRepo.GetByIdAsync(id, ct);
                    if (workout is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"Workout with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (workout.UserId != userId) return Results.Forbid();

                    await mediator.Send(new DeleteWorkoutCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteWorkout")
            .WithSummary("Soft-delete a workout by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/workouts/{id}/exercises
            group.MapPost("/{id:guid}/exercises",
                async (Guid id, ClaimsPrincipal user, [FromBody] AddExerciseRequest body,
                       IWorkoutRepository workoutRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var workout = await workoutRepo.GetByIdAsync(id, ct);
                    if (workout is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"Workout with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (workout.UserId != userId) return Results.Forbid();

                    await mediator.Send(new AddExerciseToWorkoutCommand(id, body.ExerciseId), ct);
                    return Results.NoContent();
                })
            .WithName("AddExerciseToWorkout")
            .WithSummary("Add an exercise to a workout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // DELETE /api/workouts/{id}/exercises/{exerciseId}
            group.MapDelete("/{id:guid}/exercises/{exerciseId:guid}",
                async (Guid id, Guid exerciseId, ClaimsPrincipal user,
                       IWorkoutRepository workoutRepo, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();

                    var workout = await workoutRepo.GetByIdAsync(id, ct);
                    if (workout is null)
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"Workout with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });

                    if (workout.UserId != userId) return Results.Forbid();

                    await mediator.Send(new RemoveExerciseFromWorkoutCommand(id, exerciseId), ct);
                    return Results.NoContent();
                })
            .WithName("RemoveExerciseFromWorkout")
            .WithSummary("Remove an exercise from a workout")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
        }
    }

    // Small request body model — avoids collision with CompleteWorkoutCommand constructor arguments
    public record CompleteWorkoutRequest(TimeSpan Duration);

    public record AddExerciseRequest(Guid ExerciseId);
}
