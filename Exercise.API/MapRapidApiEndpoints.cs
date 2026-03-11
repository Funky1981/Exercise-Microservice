using Exercise.Application.Features.Exercises.Commands.SyncExercises;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exercise.API
{
    public static class RapidApiEndpointMapper
    {
        public static void MapRapidApiEndpoints(this WebApplication app)
        {
            app.MapPost("/api/exercises/sync",
                async (IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(new SyncExercisesCommand(), ct);
                    return Results.Ok(new { synced = result.Added, total = result.TotalFetched });
                })
            .WithTags("Exercises")
            .WithOpenApi()
            .WithName("SyncExercisesFromExternalProvider")
            .WithSummary("Sync exercises from the configured external provider (admin only)")
            .WithDescription("Fetches exercises from the external API provider and inserts any that are not already present. Returns the count of newly added exercises and total fetched. Requires Admin role.")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
        }
    }
}
