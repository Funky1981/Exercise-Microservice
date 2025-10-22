using System;                                             
using System.Collections.Generic;                         
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;                                         
using Exercise.Application.Exercises.Dtos;                
using Exercise.Application.Features.Exercises.Queries.GetExercisesById;  
using Xunit;
using Moq;
using FluentAssertions;
using Exercise.Application.Abstractions.Repositories;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

public class GetExercisesByIdQueryHandlerTests
{
    private readonly Mock<IExerciseRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly GetExerciseByIdQueryHandler _handler;

    public GetExercisesByIdQueryHandlerTests()
    {
        _mockRepository = new Mock<IExerciseRepository>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new Exercise.Application.Features.Exercises.Mapping.ExerciseProfile());
        });
        _mapper = configuration.CreateMapper();

        _handler = new GetExerciseByIdQueryHandler(_mockRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();
        var exercise = new ExerciseEntity(
            id: exerciseId,
            name: "Push Up",
            bodyPart: "Chest",
            equipment: "None",
            targetMuscle: "Pectorals",
            gifUrl: "http://example.com/pushup.gif",
            description: "A basic push-up exercise.",
            difficulty: "Medium"
        );

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercise);

        var query = new GetExercisesByIdQuery(exerciseId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(exerciseId);
        result.Name.Should().Be("Push Up");
        result.BodyPart.Should().Be("Chest");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenExerciseDoesNotExist()
    {
        // Arrange
        var exerciseId = Guid.NewGuid();

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(exerciseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ExerciseEntity)null);

        var query = new GetExercisesByIdQuery(exerciseId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}