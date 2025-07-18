using Exercise.API;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("ExerciseDb", client =>
{
    client.BaseAddress = new Uri("https://exercisedb.p.rapidapi.com/");
    client.DefaultRequestHeaders.Add("x-rapidapi-host", builder.Configuration["RapidApi:Host"]);
    client.DefaultRequestHeaders.Add("x-rapidapi-key", builder.Configuration["RapidApi:Key"]);
});

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapExerciseEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();





