using Exercise.Application.Features.Auth.Commands.Login;
using MediatR;

namespace Exercise.API
{
    public static class MapAuthEndpoints
    {
        public static void MapAuthEndpointsRoute(this WebApplication app)
        {
            // POST /api/auth/login — public, no token required
            app.MapPost("/api/auth/login",
                async (LoginCommand command, IMediator mediator, CancellationToken ct) =>
                {
                    var response = await mediator.Send(command, ct);
                    return Results.Ok(response);
                })
            .WithTags("Auth")
            .WithOpenApi()
            .WithName("Login")
            .WithSummary("Authenticate with email and password; returns a JWT bearer token")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
