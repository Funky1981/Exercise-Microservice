using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Exercises.Mapping;
using Moq;

namespace Exercise.Application.Tests.TestHelpers
{
    /// <summary>
    /// Centralised factory for creating mocks and shared test infrastructure,
    /// so individual test classes stay lean and consistent.
    /// </summary>
    public static class MockFactory
    {
        // ?? Repository mocks ??????????????????????????????????????????????

        /// <summary>Creates a fresh, unconfigured mock of IExerciseRepository.</summary>
        public static Mock<IExerciseRepository> CreateExerciseRepositoryMock()
            => new Mock<IExerciseRepository>();

        // ?? AutoMapper ????????????????????????????????????????????????????

        /// <summary>
        /// Creates a real AutoMapper instance configured with ExerciseProfile.
        /// Re-use this across tests to avoid repeated MapperConfiguration setup.
        /// </summary>
        public static IMapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ExerciseProfile>());

            config.AssertConfigurationIsValid(); // fail fast if mapping is broken
            return config.CreateMapper();
        }
    }
}
