using Exercise.API.IntegrationTests.Infrastructure;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Exercise.API.IntegrationTests.Tests.Concurrency;

[Collection("Integration")]
public class OptimisticConcurrencyTests : IClassFixture<AuthBypassWebApplicationFactory>
{
    private readonly AuthBypassWebApplicationFactory _factory;

    public OptimisticConcurrencyTests(AuthBypassWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SaveChanges_WithStaleTrackedWorkout_ThrowsConcurrencyException()
    {
        var userId = Guid.NewGuid();
        var workoutId = Guid.NewGuid();

        using (var seedScope = _factory.Services.CreateScope())
        {
            var workoutRepository = seedScope.ServiceProvider.GetRequiredService<IWorkoutRepository>();
            var unitOfWork = seedScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await workoutRepository.AddAsync(
                new Workout(workoutId, userId, "Concurrency Seed Workout", DateTime.UtcNow, false),
                CancellationToken.None);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);
        }

        using var scope1 = _factory.Services.CreateScope();
        using var scope2 = _factory.Services.CreateScope();

        var workoutRepository1 = scope1.ServiceProvider.GetRequiredService<IWorkoutRepository>();
        var workoutRepository2 = scope2.ServiceProvider.GetRequiredService<IWorkoutRepository>();
        var unitOfWork1 = scope1.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var unitOfWork2 = scope2.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var workout1 = await workoutRepository1.GetByIdForUpdateAsync(workoutId, CancellationToken.None);
        var workout2 = await workoutRepository2.GetByIdForUpdateAsync(workoutId, CancellationToken.None);

        workout1.Should().NotBeNull();
        workout2.Should().NotBeNull();

        workout1!.Update("Scope 1 Update", DateTime.UtcNow.AddDays(1), "first save", false);
        await unitOfWork1.SaveChangesAsync(CancellationToken.None);

        workout2!.Update("Scope 2 Update", DateTime.UtcNow.AddDays(2), "stale save", false);

        var act = () => unitOfWork2.SaveChangesAsync(CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrencyException>();
    }
}
