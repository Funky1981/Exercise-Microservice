using Exercise.Application.Features.WorkoutPlans.Commands.ActivateWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.DeleteWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlanById;
using Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlansByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exercise.API
{
    public static class MapWorkoutPlanEndpoints
    {
        public static void MapWorkoutPlanEndpointsRoute(this WebApplication app)
        {
            var group = app.MapGroup("/api/workout-plans")
                           .WithTags("WorkoutPlans")
                           .WithOpenApi()
                           .RequireAuthorization();

            // GET /api/workout-plans?userId=&pageNumber=&pageSize=
            group.MapGet("/",
                async ([FromQuery] Guid userId, [FromQuery] int pageNumber, [FromQuery] int pageSize,
                       IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(
                        new GetWorkoutPlansByUserIdQuery
                        {
                            UserId = userId,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetWorkoutPlansByUserId")
            .WithSummary("Get a paged list of workout plans for a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // GET /api/workout-plans/{id}
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetWorkoutPlanByIdQuery { Id = id }, ct);
                return result is null
                    ? Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"WorkoutPlan with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    })
                    : Results.Ok(result);
            })
            .WithName("GetWorkoutPlanById")
            .WithSummary("Get a single workout plan by its ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/workout-plans
            group.MapPost("/", async (CreateWorkoutPlanCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetWorkoutPlanById", new { id }, new { id });
            })
            .WithName("CreateWorkoutPlan")
            .WithSummary("Create a new workout plan")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/workout-plans/{id}/activate
            group.MapPost("/{id:guid}/activate", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ActivateWorkoutPlanCommand { WorkoutPlanId = id }, ct);
                return Results.NoContent();
            })
            .WithName("ActivateWorkoutPlan")
            .WithSummary("Activate a workout plan")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // DELETE /api/workout-plans/{id}
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteWorkoutPlanCommand { WorkoutPlanId = id }, ct);
                return Results.NoContent();
            })
            .WithName("DeleteWorkoutPlan")
            .WithSummary("Soft-delete a workout plan by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
