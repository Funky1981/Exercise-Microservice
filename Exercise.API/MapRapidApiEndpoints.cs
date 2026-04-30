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
                async ([FromQuery] int? mediaExerciseLimit, IMediator mediator, CancellationToken ct) =>
                {
                    var result = await mediator.Send(new SyncExercisesCommand(mediaExerciseLimit), ct);
                    return Results.Ok(new
                    {
                        added = result.Added,
                        updated = result.Updated,
                        mediaEnriched = result.MediaEnriched,
                        total = result.TotalFetched
                    });
                })
            .WithTags("Exercises")
            .WithOpenApi()
            .WithName("SyncExercisesFromExternalProvider")
            .WithSummary("Sync exercises from configured catalog and media providers (admin only)")
            .WithDescription("Fetches exercises from the configured catalog providers, upserts them into the local database, and then enriches missing media from the configured media providers. Returns the counts of added, updated, media-enriched, and total fetched exercises. Requires Admin role.")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("api")
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
        }
    }
}
