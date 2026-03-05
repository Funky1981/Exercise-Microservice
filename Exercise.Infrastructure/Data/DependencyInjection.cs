using Exercise.Application.Abstractions.Repositories;
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
                    b => b.MigrationsAssembly(typeof(ExerciseDbContext).Assembly.FullName)));

            // Register repository implementations
            services.AddScoped<IExerciseRepository, ExerciseRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IWorkoutRepository, WorkoutRepository>();
            services.AddScoped<IWorkoutPlanRepository, WorkoutPlanRepository>();
            services.AddScoped<IExerciseLogRepository, ExerciseLogRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}