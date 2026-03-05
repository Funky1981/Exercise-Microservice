using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.ExerciseLogs.Dtos;
using MediatR;

namespace Exercise.Application.Features.ExerciseLogs.Queries.GetExerciseLogsByUserId
{
    public class GetExerciseLogsByUserIdQueryHandler
        : IRequestHandler<GetExerciseLogsByUserIdQuery, PagedResult<ExerciseLogDto>>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IMapper _mapper;

        public GetExerciseLogsByUserIdQueryHandler(IExerciseLogRepository exerciseLogRepository, IMapper mapper)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<ExerciseLogDto>> Handle(
            GetExerciseLogsByUserIdQuery request,
            CancellationToken cancellationToken)
        {
            var logs = await _exerciseLogRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            var dtos = _mapper.Map<List<ExerciseLogDto>>(logs);

            var paged = dtos
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<ExerciseLogDto>(paged, dtos.Count, request.PageNumber, request.PageSize);
        }
    }
}
