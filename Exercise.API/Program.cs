using Exercise.API;
using Exercise.API.Middleware;
using Exercise.Application;
using Exercise.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ?? Application layer (MediatR, AutoMapper, FluentValidation) ??????????????
builder.Services.AddApplication();

// ?? Infrastructure layer (EF Core + ExerciseRepository) ???????????????????
builder.Services.AddInfrastructure(builder.Configuration);

// ?? External HTTP client for RapidAPI (optional integration) ??????????????
builder.Services.AddHttpClient("ExerciseApi", client =>
{
    client.BaseAddress = new Uri("https://exercisedb.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("x-rapidapi-host", builder.Configuration["RapidApi:Host"]);
    client.DefaultRequestHeaders.Add("x-rapidapi-key", builder.Configuration["RapidApi:Key"]);
});

// ?? JWT Bearer Authentication ??????????????????????????????????????????????
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key 'Jwt:Key' is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ?? OpenAPI / Swagger ??????????????????????????????????????????????????????
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title   = "Exercise Microservice API",
        Version = "v1",
        Description = "A .NET 9 microservice for fitness tracking and exercise management."
    });

    // Allow JWT tokens to be entered in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token below."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ?? HTTP pipeline ??????????????????????????????????????????????????????????
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Exercise Microservice API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// ?? Minimal API endpoints ??????????????????????????????????????????????????
app.MapExerciseEndpoints();
app.MapUserEndpointsRoute();
app.MapWorkoutEndpointsRoute();
app.MapWorkoutPlanEndpointsRoute();
app.MapExerciseLogEndpointsRoute();
app.MapRapidApiEndpoints();

app.Run();





