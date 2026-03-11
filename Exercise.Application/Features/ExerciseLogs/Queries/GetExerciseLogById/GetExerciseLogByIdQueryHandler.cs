using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
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
            var log = await _exerciseLogRepository.GetOwnedByIdAsync(request.Id, request.CurrentUserId, cancellationToken);
            if (log is null)
            {
                if (await _exerciseLogRepository.ExistsAsync(request.Id, cancellationToken))
                    throw new ForbiddenException("You do not have access to this exercise log.");

                throw new NotFoundException(nameof(Exercise.Domain.Entities.ExerciseLog), request.Id);
            }

            return _mapper.Map<ExerciseLogDto>(log);
        }
    }
}
