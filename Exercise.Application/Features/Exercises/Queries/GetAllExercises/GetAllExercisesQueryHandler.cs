using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetAllExercises
{
    /// <summary>
    /// Handler for processing GetAllExercisesQuery requests
    /// </summary>
    public class GetAllExercisesQueryHandler : IRequestHandler<GetAllExercisesQuery, IReadOnlyList<ExerciseDto>>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IMapper _mapper;

        public GetAllExercisesQueryHandler(IExerciseRepository exerciseRepository, IMapper mapper)
        {
            _exerciseRepository = exerciseRepository;
            _mapper = mapper;
        }
    
        public async Task<IReadOnlyList<ExerciseDto>> Handle(GetAllExercisesQuery request, CancellationToken cancellationToken)
        {
            var exercises = await _exerciseRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<List<ExerciseDto>>(exercises);
        }
    }
 }