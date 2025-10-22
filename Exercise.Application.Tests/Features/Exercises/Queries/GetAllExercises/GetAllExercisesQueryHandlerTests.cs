using System;                                             
using System.Collections.Generic;                         
using System.Threading;                                   
using System.Threading.Tasks;                             
using AutoMapper;                                         
using Exercise.Application.Exercises.Dtos;                
using Exercise.Application.Features.Exercises.Queries.GetAllExercises;  
using Xunit;
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
    public async Task Handle_ShouldReturnAllExercises_WhenExercisesExist()
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
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercises);

        var query = new GetAllExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Push Up");
        result[1].Name.Should().Be("Squat");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoExercisesExist()
    {
        // Arrange
        var emptyList = new List<ExerciseEntity>();

        _mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        var query = new GetAllExercisesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        // Verify that the repository method was called once
        _mockRepository.Verify(
            repo => repo.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}