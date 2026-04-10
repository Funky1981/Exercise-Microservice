import { useQuery } from '@tanstack/react-query';
import { StyleSheet, Text, View } from 'react-native';
import { useLocalSearchParams } from 'expo-router';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { BarChart } from '@/components/ui/bar-chart';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { useBreakpoint } from '@/lib/responsive';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

function dateLabel(dateStr: string): string {
  const d = new Date(dateStr);
  return `${d.getDate()}/${d.getMonth() + 1}`;
}

export function ExerciseTrendScreen() {
  const { id } = useLocalSearchParams<{ id: string }>();
  const { session } = useSession();
  const { isExpanded } = useBreakpoint();

  const analyticsQuery = useQuery({
    queryKey: queryKeys.analytics.exercise(session?.userId, id),
    queryFn: () => apiClient.getExerciseAnalytics(id!),
    enabled: Boolean(session && id),
  });

  const data = analyticsQuery.data;

  const volumeData =
    data?.dataPoints.map((dp) => ({
      label: dateLabel(dp.date),
      value: dp.volume,
    })) ?? [];

  const repsData =
    data?.dataPoints.map((dp) => ({
      label: dateLabel(dp.date),
      value: dp.reps,
    })) ?? [];

  if (analyticsQuery.isLoading) {
    return (
      <AppScreen>
        <SectionHeading eyebrow="Exercise trend" title="Loading..." />
      </AppScreen>
    );
  }

  if (!data) {
    return (
      <AppScreen>
        <SectionHeading eyebrow="Exercise trend" title="No data" subtitle="Complete some workouts with this exercise to see trends." />
      </AppScreen>
    );
  }

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Exercise trend"
        title={data.exerciseName}
        subtitle="Volume and rep progression over time."
      />

      {/* Summary stats */}
      <View style={[styles.grid, isExpanded && styles.gridExpanded]}>
        <GlowCard style={styles.statCard}>
          <Text style={styles.statLabel}>Total volume</Text>
          <Text style={styles.statValue}>{data.totalVolume.toLocaleString()}</Text>
        </GlowCard>

        <GlowCard style={styles.statCard}>
          <Text style={styles.statLabel}>Total sets</Text>
          <Text style={styles.statValue}>{data.totalSets}</Text>
        </GlowCard>

        <GlowCard style={styles.statCard}>
          <Text style={styles.statLabel}>Avg reps/set</Text>
          <Text style={styles.statValue}>{data.avgRepsPerSet.toFixed(1)}</Text>
        </GlowCard>

        <GlowCard style={styles.statCard}>
          <Text style={styles.statLabel}>Avg rest</Text>
          <Text style={styles.statValue}>{Math.round(data.avgRestSeconds)}s</Text>
        </GlowCard>
      </View>

      {/* Volume over time */}
      <GlowCard>
        <Text style={styles.chartTitle}>Volume over time</Text>
        {volumeData.length > 0 ? (
          <BarChart data={volumeData} height={180} barColor={tokens.colors.accent} />
        ) : (
          <Text style={styles.emptyText}>No data points yet.</Text>
        )}
      </GlowCard>

      {/* Reps over time */}
      <GlowCard>
        <Text style={styles.chartTitle}>Reps over time</Text>
        {repsData.length > 0 ? (
          <BarChart data={repsData} height={140} barColor={tokens.colors.accentWarm} />
        ) : (
          <Text style={styles.emptyText}>No data points yet.</Text>
        )}
      </GlowCard>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  grid: { gap: tokens.spacing.md },
  gridExpanded: { flexDirection: 'row', flexWrap: 'wrap' },
  statCard: { flexBasis: 160, flexGrow: 1 },
  statLabel: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.label,
    fontSize: 11,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  statValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.display,
    fontSize: 28,
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
});
