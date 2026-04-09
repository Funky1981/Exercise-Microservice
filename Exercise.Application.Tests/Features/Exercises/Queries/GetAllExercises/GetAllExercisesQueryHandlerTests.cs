using AutoMapper;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.Exercises.Queries.GetAllExercises;
using Moq;
using FluentAssertions;
using Exercise.Application.Abstractions.Repositories;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

public class GetAllExercisesQueryHandlerTests
{
    private readonly Mock<IExerciseRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly GetAllExercisesQueryHandler _handler;

    public GetAllExercisesQueryHandlerTests()
    {
        _mockRepository = new Mock<IExerciseRepository>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new Exercise.Application.Features.Exercises.Mapping.ExerciseProfile());
        });
        _mapper = configuration.CreateMapper();

        _handler = new GetAllExercisesQueryHandler(_mockRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenExercisesExist()
    {
        // Arrange
        var exercises = new List<ExerciseEntity>
        {
            new ExerciseEntity(
                id: Guid.NewGuid(),
                name: "Push Up",
                bodyPart: "Chest",
                equipment: "None",
                targetMuscle: "Pectorals",
                gifUrl: "http://example.com/pushup.gif",
                description: "A basic push-up exercise.",
                difficulty: "Medium"
            ),
            new ExerciseEntity(
                id: Guid.NewGuid(),
                name: "Squat",
                bodyPart: "Legs",
                equipment: "None",
                targetMuscle: "Quadriceps",
                gifUrl: "http://example.com/squat.gif",
                description: "A basic squat exercise.",
                difficulty: "Medium"
            )
        };

        _mockRepository
            .Setup(repo => repo.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<ExerciseEntity>)exercises, exercises.Count));

        var query = new GetAllExercisesQuery(pageNumber: 1, pageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<PagedResult<Exercise.Application.Exercises.Dtos.ExerciseDto>>();
        result.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Push Up");
        result.Items[1].Name.Should().Be("Squat");
        result.TotalCount.Should().Be(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoExercisesExist()
    {
        // Arrange
        var emptyList = new List<ExerciseEntity>();

        _mockRepository
            .Setup(repo => repo.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(((IReadOnlyList<ExerciseEntity>)emptyList, 0));

        var query = new GetAllExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);

        _mockRepository.Verify(
            repo => repo.GetPagedAsync(
                0,
                20,
                null,
                null,
                null,
                null,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
