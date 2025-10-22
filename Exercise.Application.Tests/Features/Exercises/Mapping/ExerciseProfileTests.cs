using Xunit;
using AutoMapper;
using Exercise.Application.Features.Exercises.Mapping;
using ExerciseEntity = Exercise.Domain.Entities.Exercise;
using Exercise.Application.Exercises.Dtos;
using FluentAssertions;
using Microsoft.VisualBasic;

public class ExerciseProfileTests
{
    private readonly IMapper _mapper;

    public ExerciseProfileTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ExerciseProfile>();
        });
        _mapper = configuration.CreateMapper();
    }

    [Fact]
    public void Should_Map_Exercise_To_ExerciseDto_Correctly()
    {               
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

        // Act
        var exerciseDto = _mapper.Map<ExerciseDto>(exercise);

        // Assert
        exerciseDto.Should().NotBeNull();
        exerciseDto.Id.Should().Be(exercise.Id);
        exerciseDto.Name.Should().Be(exercise.Name);
        exerciseDto.BodyPart.Should().Be(exercise.BodyPart);
        exerciseDto.Equipment.Should().Be(exercise.Equipment);
        exerciseDto.TargetMuscle.Should().Be(exercise.TargetMuscle);
        exerciseDto.GifUrl.Should().Be(exercise.GifUrl);
        exerciseDto.Description.Should().Be(exercise.Description);
        exerciseDto.Difficulty.Should().Be(exercise.Difficulty);
    }
}