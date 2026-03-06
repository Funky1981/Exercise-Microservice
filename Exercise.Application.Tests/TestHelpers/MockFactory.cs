using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.ExerciseLogs.Mapping;
using Exercise.Application.Features.Exercises.Mapping;
using Exercise.Application.Features.Users.Mapping;
using Exercise.Application.Features.WorkoutPlans.Mapping;
using Exercise.Application.Features.Workouts.Mapping;
using Exercise.Application.Abstractions.Services;
using Moq;

namespace Exercise.Application.Tests.TestHelpers
{
    /// <summary>
    /// Centralised factory for creating mocks and shared test infrastructure,
    /// so individual test classes stay lean and consistent.
    /// </summary>
    public static class MockFactory
    {
        // Repository mocks

        /// <summary>Creates a fresh, unconfigured mock of IExerciseRepository.</summary>
        public static Mock<IExerciseRepository> CreateExerciseRepositoryMock()
            => new Mock<IExerciseRepository>();

        public static Mock<IWorkoutRepository> CreateWorkoutRepositoryMock()
            => new Mock<IWorkoutRepository>();

        public static Mock<IWorkoutPlanRepository> CreateWorkoutPlanRepositoryMock()
            => new Mock<IWorkoutPlanRepository>();

        public static Mock<IExerciseLogRepository> CreateExerciseLogRepositoryMock()
            => new Mock<IExerciseLogRepository>();

        public static Mock<IUserRepository> CreateUserRepositoryMock()
            => new Mock<IUserRepository>();

        public static Mock<IUnitOfWork> CreateUnitOfWorkMock()
        {
            var mock = new Mock<IUnitOfWork>();
            mock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            return mock;
        }

        public static Mock<ITokenService> CreateTokenServiceMock()
            => new Mock<ITokenService>();

        // AutoMapper

        /// <summary>
        /// Creates a real AutoMapper instance configured with all application profiles.
        /// </summary>
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ExerciseProfile>();
                cfg.AddProfile<WorkoutProfile>();
                cfg.AddProfile<WorkoutPlanProfile>();
                cfg.AddProfile<ExerciseLogProfile>();
                cfg.AddProfile<UserProfile>();
            });

            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        }
    }
}
