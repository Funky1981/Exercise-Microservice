using Exercise.Application.Features.Users.Commands.DeleteUser;
using Exercise.Application.Features.Users.Commands.RegisterUser;
using Exercise.Application.Features.Users.Commands.UpdateUserProfile;
using Exercise.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Exercise.API
{
    public static class MapUserEndpoints
    {
        public static void MapUserEndpointsRoute(this WebApplication app)
        {
            // Public endpoint — no RequireAuthorization so users can register
            app.MapPost("/api/users/register",
                async (RegisterUserCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    var id = await mediator.Send(command, ct);
                    return Results.Created($"/api/users/{id}", new { id });
                })
            .WithTags("Users")
            .WithOpenApi()
            .WithName("RegisterUser")
            .WithSummary("Register a new user")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            var group = app.MapGroup("/api/users")
                           .WithTags("Users")
                           .WithOpenApi()
                           .RequireAuthorization();

            // GET /api/users/{id}
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new GetUserByIdQuery(id), ct);
                return result is null
                    ? Results.NotFound(new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = $"User with id '{id}' was not found.",
                        Status = StatusCodes.Status404NotFound
                    })
                    : Results.Ok(result);
            })
            .WithName("GetUserById")
            .WithSummary("Get a user by their ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // PUT /api/users/{id}/profile
            group.MapPut("/{id:guid}/profile",
                async (Guid id, UpdateUserProfileCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    command.UserId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("UpdateUserProfile")
            .WithSummary("Update a user's profile (username, height, weight)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

            // DELETE /api/users/{id}
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
            {
                await mediator.Send(new DeleteUserCommand(id), ct);
                return Results.NoContent();
            })
            .WithName("DeleteUser")
            .WithSummary("Delete a user by their ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
