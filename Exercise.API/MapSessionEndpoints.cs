using Asp.Versioning;
using Exercise.Application.Features.Sessions.Commands.EndSession;
using Exercise.Application.Features.Sessions.Commands.LogSet;
using Exercise.Application.Features.Sessions.Commands.StartSession;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exercise.API
{
    public static class MapSessionEndpoints
    {
        public static void MapSessionEndpointsRoute(this WebApplication app)
        {
            var versionSet = app.NewApiVersionSet()
                               .HasApiVersion(new ApiVersion(1, 0))
                               .ReportApiVersions()
                               .Build();

            var group = app.MapGroup("/api/sessions")
                           .WithTags("Sessions")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // POST /api/sessions/start
            group.MapPost("/start",
                async (ClaimsPrincipal user, [FromBody] StartSessionRequest body,
                       IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId))
                        return Results.Unauthorized();

                    var logId = await mediator.Send(
                        new StartSessionCommand { UserId = userId, WorkoutId = body.WorkoutId }, ct);

                    return Results.Created($"/api/exercise-logs/{logId}", new { logId });
                })
            .WithName("StartSession")
            .WithSummary("Start a workout session linked to a workout plan")
            .WithDescription("Creates a new exercise log linked to the specified workout. Returns the logId to use for subsequent log-set and end-session calls.")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/sessions/{logId}/log-set
            group.MapPost("/{logId:guid}/log-set",
                async (Guid logId, ClaimsPrincipal user, [FromBody] LogSetRequest body,
                       IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId))
                        return Results.Unauthorized();

                    await mediator.Send(new LogSetCommand
                    {
                        LogId = logId,
                        CurrentUserId = userId,
                        ExerciseId = body.ExerciseId,
                        Reps = body.Reps,
                        DurationSeconds = body.DurationSeconds,
                        RestSeconds = body.RestSeconds
                    }, ct);

                    return Results.NoContent();
                })
            .WithName("LogSet")
            .WithSummary("Log a single set during a workout session")
            .WithDescription("Records a completed set for a specific exercise within the active session. Reps can be 0 for timed exercises.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/sessions/{logId}/end
            group.MapPost("/{logId:guid}/end",
                async (Guid logId, ClaimsPrincipal user, [FromBody] EndSessionRequest body,
                       IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId))
                        return Results.Unauthorized();

                    await mediator.Send(new EndSessionCommand
                    {
                        LogId = logId,
                        CurrentUserId = userId,
                        TotalDurationSeconds = body.TotalDurationSeconds
                    }, ct);

                    return Results.NoContent();
                })
            .WithName("EndSession")
            .WithSummary("End a workout session")
            .WithDescription("Marks the exercise log as completed and records the total session duration.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
        }

        public record StartSessionRequest(Guid WorkoutId);
        public record LogSetRequest(Guid ExerciseId, int Reps, int DurationSeconds, int RestSeconds);
        public record EndSessionRequest(int TotalDurationSeconds);
    }
}
