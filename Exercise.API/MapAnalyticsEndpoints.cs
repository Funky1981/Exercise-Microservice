using Asp.Versioning;
using Exercise.Application.Features.Analytics.Dtos;
using Exercise.Application.Features.Analytics.Queries.GetExerciseAnalytics;
using Exercise.Application.Features.Analytics.Queries.GetWeeklyAnalytics;
using Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Exercise.API
{
    public static class MapAnalyticsEndpoints
    {
        public static void MapAnalyticsEndpointsRoute(this WebApplication app)
        {
            var versionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var group = app.MapGroup("/api/analytics")
                           .WithTags("Analytics")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // GET /api/analytics/workout-summary
            // userId is derived from the JWT sub claim — users can only see their own summary
            group.MapGet("/workout-summary",
                async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId))
                        return Results.Unauthorized();

                    var result = await mediator.Send(new GetWorkoutSummaryQuery { UserId = userId }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetWorkoutSummary")
            .WithSummary("Get a workout and exercise log summary for the authenticated user")
            .WithDescription("Returns aggregated totals for the calling user: workout count, completed workouts, total duration, exercise log count, and total log duration. UserId is derived from the JWT.")
            .Produces<WorkoutSummaryDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // GET /api/analytics/weekly?weeks=12
            group.MapGet("/weekly",
                async (ClaimsPrincipal user, [FromQuery] int? weeks, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId))
                        return Results.Unauthorized();

                    var result = await mediator.Send(
                        new GetWeeklyAnalyticsQuery { UserId = userId, Weeks = weeks ?? 12 }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetWeeklyAnalytics")
            .WithSummary("Get weekly aggregated analytics for the authenticated user")
            .WithDescription("Returns volume, duration, and consistency metrics grouped by week. Defaults to 12 weeks of history.")
            .Produces<WeeklyAnalyticsDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // GET /api/analytics/exercise/{exerciseId}
            group.MapGet("/exercise/{exerciseId:guid}",
                async (Guid exerciseId, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
                {
                    if (!user.TryGetUserId(out var userId))
                        return Results.Unauthorized();

                    var result = await mediator.Send(
                        new GetExerciseAnalyticsQuery { UserId = userId, ExerciseId = exerciseId }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetExerciseAnalytics")
            .WithSummary("Get analytics for a specific exercise")
            .WithDescription("Returns volume, reps, rest time, and per-session data points for the specified exercise. Scoped to the authenticated user's completed logs.")
            .Produces<ExerciseAnalyticsDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
        }
    }
}
