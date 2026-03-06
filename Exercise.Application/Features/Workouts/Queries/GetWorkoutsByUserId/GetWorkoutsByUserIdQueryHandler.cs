using AutoMapper;
using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Models;
using Exercise.Application.Features.Workouts.Dtos;
using MediatR;

namespace Exercise.Application.Features.Workouts.Queries.GetWorkoutsByUserId
{
    public class GetWorkoutsByUserIdQueryHandler : IRequestHandler<GetWorkoutsByUserIdQuery, PagedResult<WorkoutDto>>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IMapper _mapper;

        public GetWorkoutsByUserIdQueryHandler(IWorkoutRepository workoutRepository, IMapper mapper)
        {
            _workoutRepository = workoutRepository;
            _mapper = mapper;
        }

        public async Task<PagedResult<WorkoutDto>> Handle(GetWorkoutsByUserIdQuery request, CancellationToken cancellationToken)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;
            var (workouts, totalCount) = await _workoutRepository.GetPagedByUserIdAsync(
                request.UserId, skip, request.PageSize, cancellationToken);

            var dtos = _mapper.Map<List<WorkoutDto>>(workouts);
            return new PagedResult<WorkoutDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
