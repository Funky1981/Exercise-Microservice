using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Common.Exceptions;
using Exercise.Application.Features.Analytics.Dtos;
using MediatR;

namespace Exercise.Application.Features.Analytics.Queries.GetExerciseAnalytics
{
    public class GetExerciseAnalyticsQueryHandler : IRequestHandler<GetExerciseAnalyticsQuery, ExerciseAnalyticsDto>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;
        private readonly IExerciseRepository _exerciseRepository;

        public GetExerciseAnalyticsQueryHandler(
            IExerciseLogRepository exerciseLogRepository,
            IExerciseRepository exerciseRepository)
        {
            _exerciseLogRepository = exerciseLogRepository;
            _exerciseRepository = exerciseRepository;
        }

        public async Task<ExerciseAnalyticsDto> Handle(GetExerciseAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var exercise = await _exerciseRepository.GetByIdAsync(request.ExerciseId, cancellationToken);
            if (exercise is null)
                throw new NotFoundException(nameof(exercise), request.ExerciseId);

            var logs = await _exerciseLogRepository.GetCompletedByUserIdAsync(request.UserId, 200, cancellationToken);

            // Flatten all entries for this exercise, grouped by log date
            var entries = logs
                .SelectMany(log => log.ExercisesCompleted
                    .Where(e => e.ExerciseId == request.ExerciseId)
                    .Select(e => new { log.Date, Entry = e }))
                .ToList();

            if (entries.Count == 0)
            {
                return new ExerciseAnalyticsDto
                {
                    ExerciseId = request.ExerciseId,
                    ExerciseName = exercise.Name
                };
            }

            var totalSets = entries.Sum(e => e.Entry.Sets);
            var totalReps = entries.Sum(e => e.Entry.Reps);
            var totalDurationTicks = entries
                .Where(e => e.Entry.Duration.HasValue)
                .Sum(e => e.Entry.Duration!.Value.Ticks);
            var restEntries = entries.Where(e => e.Entry.RestTime.HasValue).ToList();
            var avgRestSeconds = restEntries.Count > 0
                ? restEntries.Average(e => e.Entry.RestTime!.Value.TotalSeconds)
                : 0;

            // Group by date for data points
            var dataPoints = entries
                .GroupBy(e => e.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ExerciseDataPoint
                {
                    Date = g.Key,
                    Sets = g.Sum(e => e.Entry.Sets),
                    Reps = g.Sum(e => e.Entry.Reps),
                    Volume = g.Sum(e => e.Entry.Sets * e.Entry.Reps),
                    DurationSeconds = g.Where(e => e.Entry.Duration.HasValue)
                                       .Sum(e => e.Entry.Duration!.Value.TotalSeconds),
                    RestSeconds = g.Where(e => e.Entry.RestTime.HasValue)
                                   .Sum(e => e.Entry.RestTime!.Value.TotalSeconds)
                })
                .ToList();

            return new ExerciseAnalyticsDto
            {
                ExerciseId = request.ExerciseId,
                ExerciseName = exercise.Name,
                TotalSets = totalSets,
                TotalReps = totalReps,
                TotalVolume = totalSets * totalReps,
                AvgRepsPerSet = totalSets > 0 ? (double)totalReps / totalSets : 0,
                AvgRestSeconds = avgRestSeconds,
                TotalDuration = TimeSpan.FromTicks(totalDurationTicks),
                DataPoints = dataPoints
            };
        }
    }
}
