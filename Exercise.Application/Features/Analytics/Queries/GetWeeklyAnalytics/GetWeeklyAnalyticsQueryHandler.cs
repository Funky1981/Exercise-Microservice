using Exercise.Application.Abstractions.Repositories;
using Exercise.Application.Features.Analytics.Dtos;
using MediatR;

namespace Exercise.Application.Features.Analytics.Queries.GetWeeklyAnalytics
{
    public class GetWeeklyAnalyticsQueryHandler : IRequestHandler<GetWeeklyAnalyticsQuery, WeeklyAnalyticsDto>
    {
        private readonly IExerciseLogRepository _exerciseLogRepository;

        public GetWeeklyAnalyticsQueryHandler(IExerciseLogRepository exerciseLogRepository)
        {
            _exerciseLogRepository = exerciseLogRepository;
        }

        public async Task<WeeklyAnalyticsDto> Handle(GetWeeklyAnalyticsQuery request, CancellationToken cancellationToken)
        {
            var logs = await _exerciseLogRepository.GetCompletedByUserIdAsync(
                request.UserId, 500, cancellationToken);

            var cutoff = DateTime.UtcNow.AddDays(-7 * request.Weeks).Date;
            var recentLogs = logs.Where(l => l.Date >= cutoff).ToList();

            // Group by ISO week (Monday-start)
            var weeklyGroups = recentLogs
                .GroupBy(l => StartOfWeek(l.Date))
                .OrderBy(g => g.Key)
                .ToList();

            var weeks = weeklyGroups.Select(g =>
            {
                var allEntries = g.SelectMany(l => l.ExercisesCompleted).ToList();
                var totalSets = allEntries.Sum(e => e.Sets);
                var totalReps = allEntries.Sum(e => e.Reps);
                var restEntries = allEntries.Where(e => e.RestTime.HasValue).ToList();
                var durationSeconds = allEntries
                    .Where(e => e.Duration.HasValue)
                    .Sum(e => e.Duration!.Value.TotalSeconds);

                // Also include log-level duration for total session time
                var sessionDuration = g
                    .Where(l => l.Duration.HasValue)
                    .Sum(l => l.Duration!.Value.TotalSeconds);

                return new WeeklyDataPoint
                {
                    WeekStart = g.Key,
                    WorkoutCount = g.Count(),
                    TotalSets = totalSets,
                    TotalReps = totalReps,
                    Volume = totalSets * totalReps,
                    DurationSeconds = sessionDuration > 0 ? sessionDuration : durationSeconds,
                    AvgRestSeconds = restEntries.Count > 0
                        ? restEntries.Average(e => e.RestTime!.Value.TotalSeconds)
                        : 0
                };
            }).ToList();

            var totalVolume = weeks.Sum(w => w.Volume);
            var totalDurationSeconds = weeks.Sum(w => w.DurationSeconds);
            var weeksWithWorkouts = weeks.Count(w => w.WorkoutCount > 0);
            var avgWorkoutsPerWeek = request.Weeks > 0
                ? (double)recentLogs.Count / request.Weeks
                : 0;
            var allRestEntries = recentLogs
                .SelectMany(l => l.ExercisesCompleted)
                .Where(e => e.RestTime.HasValue)
                .ToList();
            var avgRestSeconds = allRestEntries.Count > 0
                ? allRestEntries.Average(e => e.RestTime!.Value.TotalSeconds)
                : 0;

            return new WeeklyAnalyticsDto
            {
                Weeks = weeks,
                AvgWorkoutsPerWeek = Math.Round(avgWorkoutsPerWeek, 1),
                TotalVolume = totalVolume,
                TotalDuration = TimeSpan.FromSeconds(totalDurationSeconds),
                AvgRestSeconds = Math.Round(avgRestSeconds, 1)
            };
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}
