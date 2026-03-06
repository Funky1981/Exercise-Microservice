using Asp.Versioning;
using Exercise.Application.Features.Exercises.Commands.CreateExercise;
using Exercise.Application.Features.Exercises.Commands.DeleteExercise;
using Exercise.Application.Features.Exercises.Commands.UpdateExercise;
using Exercise.Application.Features.Exercises.Queries.GetAllExercises;
using Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart;
using Exercise.Application.Features.Exercises.Queries.GetExercisesById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

            // GET /api/exercises
            group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetAllExercisesQuery(), ct);
                return Results.Ok(result);
            })
            .WithName("GetAllExercises")
            .WithSummary("Get all exercises")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

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
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // GET /api/exercises/bodypart/{bodyPart}
            group.MapGet("/bodypart/{bodyPart}", async (string bodyPart, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetExercisesByBodyPartQuery { BodyPart = bodyPart }, ct);
                return Results.Ok(result);
            })
            .WithName("GetExercisesByBodyPart")
            .WithSummary("Get exercises filtered by body part (e.g. chest, back, legs)")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // POST /api/exercises
            group.MapPost("/", async (CreateExerciseCommand command, IMediator mediator, CancellationToken ct) =>
            {
                var id = await mediator.Send(command, ct);
                return Results.CreatedAtRoute("GetExerciseById", new { id }, new { id });
            })
            .WithName("CreateExercise")
            .WithSummary("Create a new exercise")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

            // PUT /api/exercises/{id}
            group.MapPut("/{id:guid}", async (Guid id, UpdateExerciseCommand command, IMediator mediator, CancellationToken ct) =>
            {
                command.Id = id;
                await mediator.Send(command, ct);
                return Results.NoContent();
            })
            .WithName("UpdateExercise")
            .WithSummary("Update an existing exercise")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // DELETE /api/exercises/{id}
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteExerciseCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteExercise")
            .WithSummary("Delete an exercise by its ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
