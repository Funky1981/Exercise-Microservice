using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExercisesByBodyPart
{
    /// <summary>
    /// Handler for processing GetExercisesByBodyPartQuery requests
    /// </summary>
    public class GetExercisesByBodyPartQueryHandler : IRequestHandler<GetExercisesByBodyPartQuery, IReadOnlyList<ExerciseDto>>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetExercisesByBodyPartQueryHandler"/> class.
        /// </summary>
        /// <param name="exerciseRepository">The exercise repository</param>
        /// <param name="mapper">The AutoMapper instance</param>
        public GetExercisesByBodyPartQueryHandler(IExerciseRepository exerciseRepository, IMapper mapper)
        {
            _exerciseRepository = exerciseRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Handles the GetExercisesByBodyPartQuery request to retrieve exercises by body part.
        /// </summary>
        /// <param name="request">The query request containing the body part filter</param>
        /// <param name="cancellationToken">A cancellation token for the operation</param>
        /// <returns>A list of ExerciseDto objects matching the specified body part</returns>
        public async Task<IReadOnlyList<ExerciseDto>> Handle(GetExercisesByBodyPartQuery request, CancellationToken cancellationToken)
        {
            var exercises = await _exerciseRepository.GetByBodyPartAsync(request.BodyPart, cancellationToken);
            return _mapper.Map<List<ExerciseDto>>(exercises);
        }
    }
}