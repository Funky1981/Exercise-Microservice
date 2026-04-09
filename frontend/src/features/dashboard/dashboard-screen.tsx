import { useQuery } from '@tanstack/react-query';
import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
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
    queryKey: queryKeys.analytics.summary(session?.userId),
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
          Auth, session persistence, query persistence, responsive breakpoints, and live
          feature routes are now in place. Use the links below to move into plans, logs, or
          a new workout without leaving the dark-theme shell.
        </Text>
        <View style={styles.actions}>
          <PrimaryButton
            label="New workout"
            onPress={() => router.push('/(app)/workouts/new' as Href)}
            style={styles.actionButton}
          />
          <PrimaryButton
            label="Workout plans"
            onPress={() => router.push('/(app)/plans' as Href)}
            tone="muted"
            style={styles.actionButton}
          />
          <PrimaryButton
            label="Exercise logs"
            onPress={() => router.push('/(app)/logs' as Href)}
            tone="muted"
            style={styles.actionButton}
          />
        </View>
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
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionButton: {
    flexGrow: 1,
    minWidth: 160,
  },
});
