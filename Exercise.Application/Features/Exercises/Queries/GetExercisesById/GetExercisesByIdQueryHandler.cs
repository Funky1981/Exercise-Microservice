namespace Exercise.Application.Features.Exercises.Queries.GetExercisesById
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using Exercise.Application.Abstractions.Repositories;
    using Exercise.Application.Exercises.Dtos;
    using MediatR;
    public class GetExerciseByIdQueryHandler : IRequestHandler<GetExercisesByIdQuery, ExerciseDto>
    {
        private readonly IExerciseRepository _exerciseRepository;
        private readonly IMapper _mapper;

        public GetExerciseByIdQueryHandler(IExerciseRepository exerciseRepository, IMapper mapper)
        {
            _exerciseRepository = exerciseRepository;
            _mapper = mapper;
        }

        public async Task<ExerciseDto> Handle(GetExercisesByIdQuery request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(request.Id, cancellationToken);
            return _mapper.Map<ExerciseDto>(exercise);
        }
    }
}