import { useQuery } from '@tanstack/react-query';
import { StyleSheet, Text, View } from 'react-native';

import { apiClient } from '@/api/client';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { useBreakpoint } from '@/lib/responsive';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

const stats = [
  { key: 'totalWorkouts', label: 'Total workouts' },
  { key: 'completedWorkouts', label: 'Completed workouts' },
  { key: 'totalExerciseLogs', label: 'Exercise logs' },
  { key: 'completedExerciseLogs', label: 'Completed logs' },
] as const;

export function DashboardScreen() {
  const { session } = useSession();
  const { isExpanded } = useBreakpoint();
  const summaryQuery = useQuery({
    queryKey: ['analytics', 'workout-summary', session?.userId],
    queryFn: () => apiClient.getWorkoutSummary(),
    enabled: Boolean(session),
  });

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Dashboard"
        title={`Welcome back, ${session?.name ?? 'athlete'}`}
        subtitle="A dark-first control room designed to scale from phone to tablet to desktop web without changing the information architecture."
      />

      <View style={[styles.grid, isExpanded && styles.gridExpanded]}>
        {stats.map((stat) => (
          <GlowCard key={stat.key} style={styles.metricCard}>
            <Text style={styles.metricLabel}>{stat.label}</Text>
            <Text style={styles.metricValue}>
              {summaryQuery.data ? summaryQuery.data[stat.key] : '...'}
            </Text>
          </GlowCard>
        ))}
      </View>

      <GlowCard>
        <Text style={styles.panelTitle}>Current foundation</Text>
        <Text style={styles.bodyText}>
          Auth, session persistence, query persistence, responsive breakpoints, and
          backend-connected summary queries are in place. Feature modules can now build on
          stable providers rather than one-off screen state.
        </Text>
      </GlowCard>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  grid: {
    gap: tokens.spacing.md,
  },
  gridExpanded: {
    flexDirection: 'row',
    flexWrap: 'wrap',
  },
  metricCard: {
    flexBasis: 240,
    flexGrow: 1,
  },
  metricLabel: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  metricValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.display,
    fontSize: 28,
  },
  panelTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  bodyText: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 24,
  },
});
