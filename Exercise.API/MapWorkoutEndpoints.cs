using Exercise.Application.Features.Workouts.Commands.CompleteWorkout;
using Exercise.Application.Features.Workouts.Commands.CreateWorkout;
using Exercise.Application.Features.Workouts.Commands.DeleteWorkout;
using Exercise.Application.Features.Workouts.Queries.GetWorkoutById;
using Exercise.Application.Features.Workouts.Queries.GetWorkoutsByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exercise.API
{
    public static class MapWorkoutEndpoints
    {
        public static void MapWorkoutEndpointsRoute(this WebApplication app)
        {
            var group = app.MapGroup("/api/workouts")
                           .WithTags("Workouts")
                           .WithOpenApi()
                           .RequireAuthorization();

            // GET /api/workouts?userId=&pageNumber=&pageSize=
            group.MapGet("/",
                async ([FromQuery] Guid userId, [FromQuery] int pageNumber, [FromQuery] int pageSize,
                       IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(
                        new GetWorkoutsByUserIdQuery(userId, pageNumber, pageSize), ct);
                    return Results.Ok(result);
                })
            .WithName("GetWorkoutsByUserId")
            .WithSummary("Get a paged list of workouts for a user")
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
            group.MapPost("/", async (CreateWorkoutCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetWorkoutById", new { id }, new { id });
            })
            .WithName("CreateWorkout")
            .WithSummary("Create a new workout session")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
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
        }
    }

    // Small request body model — avoids collision with CompleteWorkoutCommand constructor arguments
    public record CompleteWorkoutRequest(TimeSpan Duration);
}
