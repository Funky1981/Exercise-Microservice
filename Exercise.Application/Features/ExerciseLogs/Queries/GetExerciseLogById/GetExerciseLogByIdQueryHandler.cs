using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.ExerciseLogs.Dtos;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogById
{
    public class GetExerciseLogByIdQueryHandler : IRequestHandler<GetExerciseLogByIdQuery, ExerciseLogDto?>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IMapper _mapper;

        public GetExerciseLogByIdQueryHandler(IExerciseLogRepository exerciseLogRepository, IMapper mapper)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _mapper = mapper;
        }

        public async Task<ExerciseLogDto?> Handle(GetExerciseLogByIdQuery request, CancellationToken cancellationToken)
        {
            var log = await _exerciseLogRepository.GetByIdAsync(request.Id, cancellationToken);
            return log is null ? null : _mapper.Map<ExerciseLogDto>(log);
        }
    }
}
