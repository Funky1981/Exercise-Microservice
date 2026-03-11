using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Analytics.Dtos;
using MediatR;

namespace Exercise.Application.Features.Analytics.Queries.GetWorkoutSummary
{
    public class GetWorkoutSummaryQueryHandler : IRequestHandler<GetWorkoutSummaryQuery, WorkoutSummaryDto>
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseLogRepository _exerciseLogRepository;

        public GetWorkoutSummaryQueryHandler(
            IWorkoutRepository workoutRepository,
            IExerciseLogRepository exerciseLogRepository)
        {
            _workoutRepository = workoutRepository;
            _exerciseLogRepository = exerciseLogRepository;
        }

        public async Task<WorkoutSummaryDto> Handle(GetWorkoutSummaryQuery request, CancellationToken cancellationToken)
        {
            var workouts = await _workoutRepository.GetSummaryByUserIdAsync(request.UserId, cancellationToken);
            var exerciseLogs = await _exerciseLogRepository.GetSummaryByUserIdAsync(request.UserId, cancellationToken);

            return new WorkoutSummaryDto
            {
                UserId = request.UserId,
                TotalWorkouts = workouts.TotalCount,
                CompletedWorkouts = workouts.CompletedCount,
                TotalWorkoutDuration = workouts.TotalDuration,
                TotalExerciseLogs = exerciseLogs.TotalCount,
                CompletedExerciseLogs = exerciseLogs.CompletedCount,
                TotalExerciseLogDuration = exerciseLogs.TotalDuration
            };
        }
    }
}
