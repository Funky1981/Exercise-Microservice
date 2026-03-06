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
            var workouts = await _workoutRepository.GetByUserIdAsync(request.UserId, cancellationToken);
            var exerciseLogs = await _exerciseLogRepository.GetByUserIdAsync(request.UserId, cancellationToken);

            var completedWorkouts = workouts.Where(w => w.IsCompleted).ToList();
            var completedLogs = exerciseLogs.Where(l => l.IsCompleted).ToList();

            return new WorkoutSummaryDto
            {
                UserId = request.UserId,
                TotalWorkouts = workouts.Count,
                CompletedWorkouts = completedWorkouts.Count,
                TotalWorkoutDuration = completedWorkouts
                    .Where(w => w.Duration.HasValue)
                    .Aggregate(TimeSpan.Zero, (sum, w) => sum + w.Duration!.Value),
                TotalExerciseLogs = exerciseLogs.Count,
                CompletedExerciseLogs = completedLogs.Count,
                TotalExerciseLogDuration = completedLogs
                    .Where(l => l.Duration.HasValue)
                    .Aggregate(TimeSpan.Zero, (sum, l) => sum + l.Duration!.Value)
            };
        }
    }
}
