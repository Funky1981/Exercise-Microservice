using System;                                             
using System.Collections.Generic;                         
using System.Threading;                                   
using System.Threading.Tasks;                             
using AutoMapper;                                         
using Exercise.Application.Exercises.Dtos;                
using Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart;  
using Xunit;
using Moq;
using FluentAssertions;
using Exercise.Application.Abstractions.Repositories;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;

public class GetExercisesByBodyPartQueryHandlerTests
{
    private readonly Mock<IExerciseRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly GetExercisesByBodyPartQueryHandler _handler;

    public GetExercisesByBodyPartQueryHandlerTests()
    {
        _mockRepository = new Mock<IExerciseRepository>();

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new Exercise.Application.Features.Exercises.Mapping.ExerciseProfile());
        });
        _mapper = configuration.CreateMapper();

        _handler = new GetExercisesByBodyPartQueryHandler(_mockRepository.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnExercises_WhenExercisesExistForBodyPart()
    {
        // Arrange
        var bodyPart = "Chest";
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
                name: "Bench Press",
                bodyPart: "Chest",
                equipment: "Barbell",
                targetMuscle: "Pectorals",
                gifUrl: "http://example.com/benchpress.gif",
                description: "A bench press exercise.",
                difficulty: "Hard"
            )
        };

        _mockRepository
            .Setup(repo => repo.GetByBodyPartAsync(bodyPart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercises);

        var query = new GetExercisesByBodyPartQuery { BodyPart = bodyPart };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Push Up");
        result[1].Name.Should().Be("Bench Press");

        _mockRepository.Verify(repo => repo.GetByBodyPartAsync(bodyPart, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoExercisesExistForBodyPart()
    {
        // Arrange
        var bodyPart = "Legs";
        var exercises = new List<ExerciseEntity>();

        _mockRepository
            .Setup(repo => repo.GetByBodyPartAsync(bodyPart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exercises);

        var query = new GetExercisesByBodyPartQuery { BodyPart = bodyPart };

        //Act
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();     //Result exists but is empty
        result.Should().BeEmpty();       //Zero items in the collection
        result.Should().HaveCount(0);    //Explicit count assertion

        //Verify the repository was called exactly once
        _mockRepository.Verify(repo => repo.GetByBodyPartAsync(bodyPart, It.IsAny<CancellationToken>()), Times.Once);
    }
}