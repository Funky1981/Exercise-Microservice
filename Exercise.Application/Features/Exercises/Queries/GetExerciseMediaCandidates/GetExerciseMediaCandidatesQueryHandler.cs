using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Application.Exercises.Dtos;
using MediatR;

namespace Exercise.Application.Features.Exercises.Queries.GetExerciseMediaCandidates
{
    public sealed class GetExerciseMediaCandidatesQueryHandler : IRequestHandler<GetExerciseMediaCandidatesQuery, IReadOnlyList<ExerciseMediaCandidateDto>>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IExerciseMediaCandidateRepository _candidateRepository;
        private readonly IMapper _mapper;

        public GetExerciseMediaCandidatesQueryHandler(
            IExerciseRepository exerciseRepository,
            IExerciseMediaCandidateRepository candidateRepository,
            IMapper mapper)
        {
            _exerciseRepository = exerciseRepository;
            _candidateRepository = candidateRepository;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ExerciseMediaCandidateDto>> Handle(GetExerciseMediaCandidatesQuery request, CancellationToken cancellationToken)
        {
            var exerciseExists = await _exerciseRepository.ExistsAsync(request.ExerciseId, cancellationToken);
            if (!exerciseExists)
            {
                throw new NotFoundException(nameof(Exercise.Domain.Entities.Exercise), request.ExerciseId);
            }

            var candidates = await _candidateRepository.GetByExerciseIdAsync(request.ExerciseId, cancellationToken);
            return _mapper.Map<List<ExerciseMediaCandidateDto>>(candidates);
        }
    }
}