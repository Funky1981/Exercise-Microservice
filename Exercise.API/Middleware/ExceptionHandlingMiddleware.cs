using Exercise.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Exercise.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation failure for request {Path}", context.Request.Path);
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found for request {Path}", context.Request.Path);
                await HandleNotFoundExceptionAsync(context, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt for request {Path}", context.Request.Path);
                await HandleUnauthorizedExceptionAsync(context, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);
                await HandleGenericExceptionAsync(context);
            }
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Instance = context.Request.Path
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        private static Task HandleNotFoundExceptionAsync(HttpContext context, NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Resource not found.",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Instance = context.Request.Path
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        private static Task HandleUnauthorizedExceptionAsync(HttpContext context, UnauthorizedAccessException ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "Unauthorized.",
                Detail = ex.Message,
                Status = StatusCodes.Status401Unauthorized,
                Instance = context.Request.Path
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }

        private static Task HandleGenericExceptionAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Detail = "Please try again later.",
                Status = StatusCodes.Status500InternalServerError,
                Instance = context.Request.Path
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
