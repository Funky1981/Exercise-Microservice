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
            services.AddScoped<IExerciseMediaCandidateRepository, ExerciseMediaCandidateRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
            services.AddScoped<IExerciseLogRepository, ExerciseLogRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.Configure<ExerciseProviderOptions>(configuration.GetSection(ExerciseProviderOptions.SectionName));

            services.AddScoped<IExerciseCatalogProvider, RapidApiExerciseProvider>();
            services.AddScoped<IExerciseCatalogProvider, WgerExerciseProvider>();
            services.AddScoped<CompositeExerciseDataProvider>();
            services.AddScoped<IExerciseDataProvider>(serviceProvider => serviceProvider.GetRequiredService<CompositeExerciseDataProvider>());

            services.AddScoped<IExerciseMediaProvider, OpenverseExerciseMediaProvider>();
            services.AddScoped<IExerciseMediaProvider, WikimediaCommonsExerciseMediaProvider>();
            services.AddScoped<IExerciseMediaProvider, PexelsExerciseMediaProvider>();
            services.AddScoped<IExerciseMediaProvider, WgerExerciseMediaProvider>();
            services.AddScoped<IExerciseMediaProvider, CuratedManifestExerciseMediaProvider>();
            services.AddScoped<IExerciseMediaEnrichmentService, ExerciseMediaEnrichmentService>();

            services.AddHttpClient("RapidApiExerciseApi", client =>
            {
                client.BaseAddress = new Uri("https://exercisedb.p.rapidapi.com/");
                client.DefaultRequestHeaders.Add("x-rapidapi-host", configuration["RapidApi:Host"]);
                client.DefaultRequestHeaders.Add("x-rapidapi-key", configuration["RapidApi:Key"]);
            }).AddStandardResilienceHandler();

            services.AddHttpClient("WgerExerciseApi", client =>
            {
                client.BaseAddress = new Uri(configuration["Wger:BaseUrl"] ?? "https://wger.de/api/v2/");
                if (!string.IsNullOrWhiteSpace(configuration["Wger:ApiKey"]))
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Token {configuration["Wger:ApiKey"]}");
                }
            }).AddStandardResilienceHandler();

            services.AddHttpClient("OpenverseExerciseApi", client =>
            {
                client.BaseAddress = new Uri(configuration["Openverse:BaseUrl"] ?? "https://api.openverse.org/v1/");
            }).AddStandardResilienceHandler();

            services.AddHttpClient("WikimediaCommonsApi", client =>
            {
                client.BaseAddress = new Uri(configuration["WikimediaCommons:BaseUrl"] ?? "https://commons.wikimedia.org/w/");
                client.DefaultRequestHeaders.Add("User-Agent", "ExerciseMicroservice/1.0 (+https://github.com/Funky1981/Exercise-Microservice)");
            }).AddStandardResilienceHandler();

            services.AddHttpClient("PexelsExerciseApi", client =>
            {
                client.BaseAddress = new Uri(configuration["Pexels:BaseUrl"] ?? "https://api.pexels.com/");
                if (!string.IsNullOrWhiteSpace(configuration["Pexels:ApiKey"]))
                {
                    client.DefaultRequestHeaders.Add("Authorization", configuration["Pexels:ApiKey"]);
                }
            }).AddStandardResilienceHandler();

            services.AddHostedService<LegacyExerciseSeedCleanupService>();

            return services;
        }
    }
}
