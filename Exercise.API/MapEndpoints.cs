using Asp.Versioning;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Exercises.Dtos;
using Exercise.Application.Features.Exercises.Commands.CreateExercise;
using Exercise.Application.Features.Exercises.Commands.DeleteExercise;
using Exercise.Application.Features.Exercises.Commands.ReviewExerciseMediaCandidate;
using Exercise.Application.Features.Exercises.Commands.UpdateExercise;
using Exercise.Application.Features.Exercises.Queries.GetAllExercises;
using Exercise.Application.Features.Exercises.Queries.GetExerciseFilters;
using Exercise.Application.Features.Exercises.Queries.GetExerciseMediaCandidates;
using Exercise.Application.Features.Exercises.Queries.GetPendingExerciseMediaCandidates;
using Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart;
using Exercise.Application.Features.Exercises.Queries.GetExercisesById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Exercise.API
{
    public static class MapEndpoints
    {
        private const string RapidApiExternalIdPrefix = "rapidapi:";

        public static void MapExerciseEndpoints(this WebApplication app)
        {
            var versionSet = app.NewApiVersionSet()
                               .HasApiVersion(new ApiVersion(1, 0))
                               .ReportApiVersions()
                               .Build();

            var group = app.MapGroup("/api/exercises")
                           .WithTags("Exercises")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // GET /api/exercises?pageNumber=1&pageSize=20
            group.MapGet("/",
                async (IMediator mediator, CancellationToken ct,
                       [FromQuery] int pageNumber = 1,
                       [FromQuery] int pageSize = 20,
                       [FromQuery] string? region = null,
                       [FromQuery] string? bodyPart = null,
                       [FromQuery] string? equipment = null,
                       [FromQuery] string? search = null,
                       [FromQuery] bool mediaOnly = false) =>
                {
                    var result = await mediator.Send(new GetAllExercisesQuery(pageNumber, pageSize)
                    {
                        Region = region,
                        BodyPart = bodyPart,
                        Equipment = equipment,
                        Search = search,
                        MediaOnly = mediaOnly,
                    }, ct);
                    return Results.Ok(result);
                })
            .WithName("GetAllExercises")
            .WithSummary("Get all exercises (paged)")
            .WithDescription("Returns a paged catalogue of exercises. Optional filters: region, bodyPart, equipment, search, and mediaOnly.")
            .Produces<PagedResult<ExerciseDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .CacheOutput("ExerciseCatalogue");

            group.MapGet("/filters", async (IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetExerciseFiltersQuery(), ct);
                return Results.Ok(result);
            })
            .WithName("GetExerciseFilters")
            .WithSummary("Get exercise filter metadata")
            .Produces<ExerciseFiltersDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .CacheOutput("ExerciseCatalogue");

            group.MapGet("/media-candidates/review", async (IMediator mediator, CancellationToken ct, [FromQuery] int limit = 100) =>
            {
                var result = await mediator.Send(new GetPendingExerciseMediaCandidatesQuery(limit), ct);
                return Results.Ok(result);
            })
            .WithName("GetPendingExerciseMediaCandidates")
            .WithSummary("Get pending media review candidates across exercises (Admin only)")
            .RequireAuthorization("Admin")
            .Produces<IReadOnlyList<ExerciseMediaReviewQueueItemDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

            // GET /api/exercises/{id}
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetExercisesByIdQuery(id), ct);
                return result is null
                    ? Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"Exercise with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    })
                    : Results.Ok(result);
            })
            .WithName("GetExerciseById")
            .WithSummary("Get a single exercise by its ID")
            .Produces<ExerciseDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // GET /api/exercises/{id}/rapidapi-image?resolution=180
            group.MapGet("/{id:guid}/rapidapi-image",
                async (Guid id, IExerciseRepository exerciseRepository, IHttpClientFactory httpClientFactory, CancellationToken ct,
                       [FromQuery] int resolution = 180) =>
                {
                    var exercise = await exerciseRepository.GetByIdAsync(id, ct);
                    if (exercise is null)
                    {
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "Resource not found.",
                            Detail = $"Exercise with id '{id}' was not found.",
                            Status = StatusCodes.Status404NotFound
                        });
                    }

                    if (string.IsNullOrWhiteSpace(exercise.ExternalId)
                        || !exercise.ExternalId.StartsWith(RapidApiExternalIdPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "RapidAPI image unavailable.",
                            Detail = "This exercise was not sourced from RapidAPI ExerciseDB, so it cannot use the RapidAPI image endpoint.",
                            Status = StatusCodes.Status400BadRequest
                        });
                    }

                    var rapidApiExerciseId = exercise.ExternalId[RapidApiExternalIdPrefix.Length..];
                    if (string.IsNullOrWhiteSpace(rapidApiExerciseId))
                    {
                        return Results.BadRequest(new ProblemDetails
                        {
                            Title = "RapidAPI image unavailable.",
                            Detail = "This exercise does not have a valid RapidAPI ExerciseDB exercise id.",
                            Status = StatusCodes.Status400BadRequest
                        });
                    }

                    var safeResolution = resolution <= 0 ? 180 : resolution;
                    var client = httpClientFactory.CreateClient("RapidApiExerciseApi");
                    using var response = await client.GetAsync($"image?exerciseId={Uri.EscapeDataString(rapidApiExerciseId)}&resolution={safeResolution}", ct);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return Results.NotFound(new ProblemDetails
                        {
                            Title = "RapidAPI image not found.",
                            Detail = $"No RapidAPI image was found for exercise id '{rapidApiExerciseId}'.",
                            Status = StatusCodes.Status404NotFound
                        });
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        return Results.Problem(
                            title: "RapidAPI image request failed.",
                            detail: $"RapidAPI returned status code {(int)response.StatusCode}.",
                            statusCode: StatusCodes.Status502BadGateway);
                    }

                    var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/gif";
                    var bytes = await response.Content.ReadAsByteArrayAsync(ct);
                    return Results.File(bytes, contentType);
                })
            .WithName("GetRapidApiExerciseImage")
            .WithSummary("Proxy the RapidAPI ExerciseDB image endpoint for an exercise")
            .WithDescription("Uses the stored rapidapi:{exerciseId} external id to call /image?exerciseId={id}&resolution={resolution} without exposing the RapidAPI key to the frontend.")
            .Produces(StatusCodes.Status200OK, contentType: "image/gif")
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status502BadGateway)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}/media-candidates", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetExerciseMediaCandidatesQuery(id), ct);
                return Results.Ok(result);
            })
            .WithName("GetExerciseMediaCandidates")
            .WithSummary("Get candidate media matches for an exercise (Admin only)")
            .RequireAuthorization("Admin")
            .Produces<IReadOnlyList<ExerciseMediaCandidateDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

            group.MapPost("/{id:guid}/media-candidates/{candidateId:guid}/approve",
                async (Guid id, Guid candidateId, [FromBody] ReviewExerciseMediaCandidateRequest? request, IMediator mediator, IOutputCacheStore cache, CancellationToken ct) =>
                {
                    await mediator.Send(new ReviewExerciseMediaCandidateCommand(id, candidateId, true, request?.Notes), ct);
                    await cache.EvictByTagAsync("exercises", ct);
                    return Results.NoContent();
                })
            .WithName("ApproveExerciseMediaCandidate")
            .WithSummary("Approve a candidate media match for an exercise (Admin only)")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

            group.MapPost("/{id:guid}/media-candidates/{candidateId:guid}/reject",
                async (Guid id, Guid candidateId, [FromBody] ReviewExerciseMediaCandidateRequest? request, IMediator mediator, IOutputCacheStore cache, CancellationToken ct) =>
                {
                    await mediator.Send(new ReviewExerciseMediaCandidateCommand(id, candidateId, false, request?.Notes), ct);
                    await cache.EvictByTagAsync("exercises", ct);
                    return Results.NoContent();
                })
            .WithName("RejectExerciseMediaCandidate")
            .WithSummary("Reject a candidate media match for an exercise (Admin only)")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

            // GET /api/exercises/bodypart/{bodyPart}
            group.MapGet("/bodypart/{bodyPart}", async (string bodyPart, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetExercisesByBodyPartQuery { BodyPart = bodyPart }, ct);
                return Results.Ok(result);
            })
            .WithName("GetExercisesByBodyPart")
            .WithSummary("Get exercises filtered by body part (e.g. chest, back, legs)")
            .WithDescription("Valid body part values include: chest, back, lower arms, lower legs, neck, shoulders, upper arms, upper legs, waist, cardio.")
            .Produces<IReadOnlyList<ExerciseDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // POST /api/exercises (Admin only)
            group.MapPost("/", async (CreateExerciseCommand command, IMediator mediator,
                                     IOutputCacheStore cache, CancellationToken ct) =>
            {
                var id = await mediator.Send(command, ct);
                await cache.EvictByTagAsync("exercises", ct);
                return Results.CreatedAtRoute("GetExerciseById", new { id }, new { id });
            })
            .WithName("CreateExercise")
            .WithSummary("Create a new exercise (Admin only)")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

            // PUT /api/exercises/{id} (Admin only)
            group.MapPut("/{id:guid}", async (Guid id, UpdateExerciseCommand command, IMediator mediator,
                                              IOutputCacheStore cache, CancellationToken ct) =>
            {
                command.Id = id;
                await mediator.Send(command, ct);
                await cache.EvictByTagAsync("exercises", ct);
                return Results.NoContent();
            })
            .WithName("UpdateExercise")
            .WithSummary("Update an existing exercise (Admin only)")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

            // DELETE /api/exercises/{id} (Admin only)
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator,
                                                 IOutputCacheStore cache, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteExerciseCommand(id), ct);
                await cache.EvictByTagAsync("exercises", ct);
                return Results.NoContent();
            })
            .WithName("DeleteExercise")
            .WithSummary("Delete an exercise by its ID (Admin only)")
            .RequireAuthorization("Admin")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
        }
    }

    public sealed record ReviewExerciseMediaCandidateRequest(string? Notes);
}
