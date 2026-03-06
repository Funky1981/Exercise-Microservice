using Asp.Versioning;
using Exercise.Application.Features.WorkoutPlans.Commands.ActivateWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Dtos;
using Exercise.Application.Features.WorkoutPlans.Commands.AddWorkoutToWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.CreateWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.DeleteWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.RemoveWorkoutFromWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Commands.UpdateWorkoutPlan;
using Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlanById;
using Exercise.Application.Features.WorkoutPlans.Queries.GetWorkoutPlansByUserId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exercise.API
{
    public static class MapWorkoutPlanEndpoints
    {
        public static void MapWorkoutPlanEndpointsRoute(this WebApplication app)
        {
            var versionSet = app.NewApiVersionSet()
                               .HasApiVersion(new ApiVersion(1, 0))
                               .ReportApiVersions()
                               .Build();

            var group = app.MapGroup("/api/workout-plans")
                           .WithTags("WorkoutPlans")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // GET /api/workout-plans?pageNumber=&pageSize=
            // userId is derived from the JWT sub claim — users can only see their own plans
            group.MapGet("/",
                async (ClaimsPrincipal user, [FromQuery] int pageNumber, [FromQuery] int pageSize,
                       IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId))
                        return Results.Unauthorized();

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
            .WithSummary("Get a paged list of the authenticated user's workout plans")
            .WithDescription("pageNumber and pageSize are required. Results are scoped to the authenticated user's JWT — users can only see their own plans.")
            .Produces<IReadOnlyList<WorkoutPlanDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

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
            .Produces<WorkoutPlanDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/workout-plans
            // userId is set from the JWT claim — the request body UserId field is ignored
            group.MapPost("/", async (ClaimsPrincipal user, CreateWorkoutPlanCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var userId))
                    return Results.Unauthorized();

                command.UserId = userId;
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetWorkoutPlanById", new { id }, new { id });
            })
            .WithName("CreateWorkoutPlan")
            .WithSummary("Create a new workout plan for the authenticated user")
            .WithDescription("The userId is extracted from the JWT claim and applied automatically — any UserId in the request body is ignored.")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // PUT /api/workout-plans/{id}
            group.MapPut("/{id:guid}",
                async (Guid id, UpdateWorkoutPlanCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    command.WorkoutPlanId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("UpdateWorkoutPlan")
            .WithSummary("Update a workout plan's name, dates, and notes")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/workout-plans/{id}/activate
            group.MapPost("/{id:guid}/activate", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new ActivateWorkoutPlanCommand { WorkoutPlanId = id }, ct);
                return Results.NoContent();
            })
            .WithName("ActivateWorkoutPlan")
            .WithSummary("Activate a workout plan")
            .WithDescription("Sets the plan's status to Active. Only one plan should be active at a time.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // DELETE /api/workout-plans/{id}
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteWorkoutPlanCommand { WorkoutPlanId = id }, ct);
                return Results.NoContent();
            })
            .WithName("DeleteWorkoutPlan")
            .WithSummary("Soft-delete a workout plan by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/workout-plans/{id}/workouts
            group.MapPost("/{id:guid}/workouts",
                async (Guid id, [FromBody] AddWorkoutRequest body, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new AddWorkoutToWorkoutPlanCommand(id, body.WorkoutId), ct);
                    return Results.NoContent();
                })
            .WithName("AddWorkoutToWorkoutPlan")
            .WithSummary("Add a workout to a workout plan")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // DELETE /api/workout-plans/{id}/workouts/{workoutId}
            group.MapDelete("/{id:guid}/workouts/{workoutId:guid}",
                async (Guid id, Guid workoutId, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new RemoveWorkoutFromWorkoutPlanCommand(id, workoutId), ct);
                    return Results.NoContent();
                })
            .WithName("RemoveWorkoutFromWorkoutPlan")
            .WithSummary("Remove a workout from a workout plan")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
        }
    }

    public record AddWorkoutRequest(Guid WorkoutId);
}
