using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Abstractions.Services;
using Exercise.Infrastructure.ExternalApis;
using Exercise.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Exercise.Infrastructure.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ExerciseDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b
                        .MigrationsAssembly(typeof(ExerciseDbContext).Assembly.FullName)
                        .EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)));

            // Register repository implementations
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
            services.AddScoped<IExerciseLogRepository, ExerciseLogRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // External API provider — swap implementation here to change providers
            services.AddScoped<IExerciseDataProvider, RapidApiExerciseProvider>();

            // Named HttpClient for RapidAPI (with standard resilience pipeline)
            services.AddHttpClient("ExerciseApi", client =>
            {
                client.BaseAddress = new Uri("https://exercisedb.p.rapidapi.com/");
                client.DefaultRequestHeaders.Add("x-rapidapi-host", configuration["RapidApi:Host"]);
                client.DefaultRequestHeaders.Add("x-rapidapi-key", configuration["RapidApi:Key"]);
            }).AddStandardResilienceHandler();

            return services;
        }
    }
}