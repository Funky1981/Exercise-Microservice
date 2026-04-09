using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Exercises.Dtos;
using Exercise.Application.Features.Exercises.Support;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExerciseFilters
{
    public class GetExerciseFiltersQueryHandler : IRequestHandler<GetExerciseFiltersQuery, ExerciseFiltersDto>
    {
        private readonly IExerciseRepository _exerciseRepository;

        public GetExerciseFiltersQueryHandler(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        public async Task<ExerciseFiltersDto> Handle(
            GetExerciseFiltersQuery request,
            CancellationToken cancellationToken)
        {
            var exercises = await _exerciseRepository.GetAllAsync(cancellationToken);
            var bodyParts = exercises.Select(exercise => exercise.BodyPart).ToList();
            var equipment = exercises
                .Select(exercise => exercise.Equipment)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value)
                .ToList();

            return new ExerciseFiltersDto
            {
                Regions = ExerciseRegionCatalog.Regions,
                BodyPartsByRegion = ExerciseRegionCatalog.BuildBodyPartsByRegion(bodyParts),
                Equipment = equipment,
            };
        }
    }
}
