using Asp.Versioning;
using Exercise.Application.Common.Models;
using Exercise.Application.Exercises.Dtos;
using Exercise.Application.Features.Exercises.Commands.CreateExercise;
using Exercise.Application.Features.Exercises.Commands.DeleteExercise;
using Exercise.Application.Features.Exercises.Commands.UpdateExercise;
using Exercise.Application.Features.Exercises.Queries.GetAllExercises;
using Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart;
using Exercise.Application.Features.Exercises.Queries.GetExercisesById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace Exercise.API
{
    public static class MapEndpoints
    {
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
                       [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20) =>
                {
                    var result = await mediator.Send(new GetAllExercisesQuery(pageNumber, pageSize), ct);
                    return Results.Ok(result);
                })
            .WithName("GetAllExercises")
            .WithSummary("Get all exercises (paged)")
            .WithDescription("Returns a paged catalogue of exercises. Use pageNumber and pageSize query params (default: page 1, size 20).")
            .Produces<PagedResult<ExerciseDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .CacheOutput("ExerciseCatalogue");

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
}
