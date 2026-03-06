using Asp.Versioning;
using Exercise.Application.Features.Users.Commands.DeleteUser;
using Exercise.Application.Features.Users.Dtos;
using Exercise.Application.Features.Users.Commands.RegisterUser;
using Exercise.Application.Features.Users.Commands.UpdateUserProfile;
using Exercise.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            .WithDescription("Creates a new user account. Returns the new user's ID. No authentication required.")
            .Produces(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

            var versionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1, 0))
                .ReportApiVersions()
                .Build();

            var group = app.MapGroup("/api/users")
                           .WithTags("Users")
                           .WithOpenApi()
                           .RequireAuthorization()
                           .RequireRateLimiting("api")
                           .WithApiVersionSet(versionSet)
                           .HasApiVersion(new ApiVersion(1, 0));

            // GET /api/users/{id}
            group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
            {
                var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();
                if (id != userId) return Results.Forbid();

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
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // PUT /api/users/{id}/profile
            group.MapPut("/{id:guid}/profile",
                async (Guid id, ClaimsPrincipal user, UpdateUserProfileCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();
                    if (id != userId) return Results.Forbid();

                    command.UserId = id;
                    await mediator.Send(command, ct);
                    return Results.NoContent();
                })
            .WithName("UpdateUserProfile")
            .WithSummary("Update a user's profile (username, height, weight)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

            // DELETE /api/users/{id}
            group.MapDelete("/{id:guid}",
                async (Guid id, ClaimsPrincipal user, IMediator mediator, CancellationToken ct) =>
                {
                    var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
                    if (!Guid.TryParse(sub, out var userId)) return Results.Unauthorized();
                    if (id != userId) return Results.Forbid();

                    await mediator.Send(new DeleteUserCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteUser")
            .WithSummary("Delete a user by their ID")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);
        }
    }
}
