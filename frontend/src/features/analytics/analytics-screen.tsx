import { useQuery } from '@tanstack/react-query';
import { StyleSheet, Text, View } from 'react-native';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { BarChart } from '@/components/ui/bar-chart';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { useBreakpoint } from '@/lib/responsive';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

function formatDuration(isoOrSeconds: string | number): string {
  const totalSeconds =
    typeof isoOrSeconds === 'number'
      ? isoOrSeconds
      : parseDurationToSeconds(isoOrSeconds);
  const h = Math.floor(totalSeconds / 3600);
  const m = Math.floor((totalSeconds % 3600) / 60);
  if (h > 0) return `${h}h ${m}m`;
  return `${m}m`;
}

function parseDurationToSeconds(iso: string): number {
  // "HH:MM:SS" or "d.HH:MM:SS"
  const parts = iso.split(':');
  if (parts.length < 2) return 0;
  const hours = parseInt(parts[0], 10) || 0;
  const minutes = parseInt(parts[1], 10) || 0;
  const seconds = parseInt(parts[2]?.split('.')[0] ?? '0', 10) || 0;
  return hours * 3600 + minutes * 60 + seconds;
}

function weekLabel(dateStr: string): string {
  const d = new Date(dateStr);
  const month = d.toLocaleString('en', { month: 'short' });
  return `${d.getDate()} ${month}`;
}

export function AnalyticsScreen() {
  const { session } = useSession();
  const { isExpanded } = useBreakpoint();

  const summaryQuery = useQuery({
    queryKey: queryKeys.analytics.summary(session?.userId),
    queryFn: () => apiClient.getWorkoutSummary(),
    enabled: Boolean(session),
  });

  const weeklyQuery = useQuery({
    queryKey: queryKeys.analytics.weekly(session?.userId, 12),
    queryFn: () => apiClient.getWeeklyAnalytics(12),
    enabled: Boolean(session),
  });

  const summary = summaryQuery.data;
  const weekly = weeklyQuery.data;

  const volumeData =
    weekly?.weeks.map((w) => ({
      label: weekLabel(w.weekStart),
      value: w.volume,
    })) ?? [];

  const workoutCountData =
    weekly?.weeks.map((w) => ({
      label: weekLabel(w.weekStart),
      value: w.workoutCount,
    })) ?? [];

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Analytics"
        title="Your progress"
        subtitle="Volume, consistency, and training trends over the last 12 weeks."
      />

      {/* Summary cards */}
      <View style={[styles.grid, isExpanded && styles.gridExpanded]}>
        <GlowCard style={styles.metricCard}>
          <Text style={styles.metricLabel}>Total volume</Text>
          <Text style={styles.metricValue}>
            {weekly?.totalVolume.toLocaleString() ?? '...'}
          </Text>
          <Text style={styles.metricSub}>sets × reps</Text>
        </GlowCard>

        <GlowCard style={styles.metricCard}>
          <Text style={styles.metricLabel}>Total time</Text>
          <Text style={styles.metricValue}>
            {weekly ? formatDuration(weekly.totalDuration) : '...'}
          </Text>
        </GlowCard>

        <GlowCard style={styles.metricCard}>
          <Text style={styles.metricLabel}>Avg rest</Text>
          <Text style={styles.metricValue}>
            {weekly ? `${Math.round(weekly.avgRestSeconds)}s` : '...'}
          </Text>
        </GlowCard>

        <GlowCard style={styles.metricCard}>
          <Text style={styles.metricLabel}>Consistency</Text>
          <Text style={styles.metricValue}>
            {weekly ? `${weekly.avgWorkoutsPerWeek}` : '...'}
          </Text>
          <Text style={styles.metricSub}>workouts / week</Text>
        </GlowCard>
      </View>

      {/* Weekly volume chart */}
      <GlowCard>
        <Text style={styles.chartTitle}>Weekly volume</Text>
        {volumeData.length > 0 ? (
          <BarChart data={volumeData} height={180} barColor={tokens.colors.accent} />
        ) : (
          <Text style={styles.emptyText}>
            {weeklyQuery.isLoading ? 'Loading...' : 'No workout data yet.'}
          </Text>
        )}
      </GlowCard>

      {/* Weekly workout count chart */}
      <GlowCard>
        <Text style={styles.chartTitle}>Workouts per week</Text>
        {workoutCountData.length > 0 ? (
          <BarChart
            data={workoutCountData}
            height={140}
            barColor={tokens.colors.accentWarm}
          />
        ) : (
          <Text style={styles.emptyText}>
            {weeklyQuery.isLoading ? 'Loading...' : 'Complete workouts to see trends.'}
          </Text>
        )}
      </GlowCard>

      {/* Lifetime summary */}
      <GlowCard>
        <Text style={styles.chartTitle}>Lifetime</Text>
        <View style={[styles.grid, isExpanded && styles.gridExpanded]}>
          <View style={styles.lifetimeStat}>
            <Text style={styles.metricLabel}>Workouts</Text>
            <Text style={styles.metricValue}>{summary?.totalWorkouts ?? '...'}</Text>
          </View>
          <View style={styles.lifetimeStat}>
            <Text style={styles.metricLabel}>Completed</Text>
            <Text style={styles.metricValue}>{summary?.completedWorkouts ?? '...'}</Text>
          </View>
          <View style={styles.lifetimeStat}>
            <Text style={styles.metricLabel}>Logs</Text>
            <Text style={styles.metricValue}>{summary?.totalExerciseLogs ?? '...'}</Text>
          </View>
          <View style={styles.lifetimeStat}>
            <Text style={styles.metricLabel}>Total time</Text>
            <Text style={styles.metricValue}>
              {summary ? formatDuration(summary.totalExerciseLogDuration) : '...'}
            </Text>
          </View>
        </View>
      </GlowCard>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  grid: { gap: tokens.spacing.md },
  gridExpanded: { flexDirection: 'row', flexWrap: 'wrap' },
  metricCard: { flexBasis: 200, flexGrow: 1 },
  metricLabel: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.label,
    fontSize: 11,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  metricValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.display,
    fontSize: 28,
  },
  metricSub: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 12,
  },
  chartTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  emptyText: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 14,
    textAlign: 'center',
    paddingVertical: tokens.spacing.lg,
  },
  lifetimeStat: {
    flexBasis: 120,
    flexGrow: 1,
    gap: tokens.spacing.xs,
  },
});
