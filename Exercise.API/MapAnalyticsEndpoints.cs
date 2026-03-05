using Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary;
using MediatR;

namespace Exercise.API
{
    public static class MapAnalyticsEndpoints
    {
        public static void MapAnalyticsEndpointsRoute(this WebApplication app)
        {
            var group = app.MapGroup("/api/analytics")
                           .WithTags("Analytics")
                           .WithOpenApi()
                           .RequireAuthorization();

            // GET /api/analytics/workout-summary?userId={userId}
            group.MapGet("/workout-summary",
                async ([Microsoft.AspNetCore.Mvc.FromQuery] Guid userId, IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(new GetWorkoutSummaryQuery { UserId = userId }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetWorkoutSummary")
            .WithSummary("Get a workout and exercise log summary for a user")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
