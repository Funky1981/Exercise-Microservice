using Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary;
using MediatR;
using System.Security.Claims;

namespace Exercise.API
{
    public static class MapAnalyticsEndpoints
    {
        public static void MapAnalyticsEndpointsRoute(this WebApplication app)
        {
            var group = app.MapGroup("/api/analytics")
                           .WithTags("Analytics")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api");

            // GET /api/analytics/workout-summary
            // userId is derived from the JWT sub claim — users can only see their own summary
            group.MapGet("/workout-summary",
                async (ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId))
                        return Results.Unauthorized();

                    var result = await mediator.Send(new GetWorkoutSummaryQuery { UserId = userId }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetWorkoutSummary")
            .WithSummary("Get a workout and exercise log summary for the authenticated user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
