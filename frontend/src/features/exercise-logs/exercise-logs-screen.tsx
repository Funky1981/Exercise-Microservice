import { FlatList, StyleSheet, Text } from 'react-native';
import { router, type Href } from 'expo-router';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { ExerciseLog } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { formatDate, formatDuration } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export function ExerciseLogsScreen() {
  const { session } = useSession();
  const logsQuery = useQuery({
    queryKey: queryKeys.exerciseLogs.list(session?.userId, 1, 20),
    queryFn: () => apiClient.getExerciseLogs(),
    enabled: Boolean(session?.userId),
  });

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Tracking"
        title="Exercise logs"
        subtitle="Logs are where set-by-set entries live, so the detail screen focuses on adding entries and closing out a session cleanly."
      />

      <PrimaryButton
        label="Create log"
        onPress={() => router.push('/(app)/logs/new' as Href)}
      />

      {logsQuery.isPending ? (
        <StatusCard title="Loading logs" body="Fetching the latest log list." busy />
      ) : logsQuery.isError ? (
        <StatusCard
          title="Unable to load logs"
          body={logsQuery.error instanceof Error ? logsQuery.error.message : 'Try again in a moment.'}
        />
      ) : (
        <FlatList
          data={logsQuery.data?.items ?? []}
          keyExtractor={(item) => item.id}
          contentContainerStyle={styles.list}
          renderItem={({ item }) => <ExerciseLogCard log={item} />}
          ListEmptyComponent={
            <StatusCard
              title="No logs yet"
              body="Create an exercise log to start tracking sets, reps, and time."
            />
          }
          scrollEnabled={false}
        />
      )}
    </AppScreen>
  );
}

function ExerciseLogCard({ log }: { log: ExerciseLog }) {
  return (
    <GlowCard>
      <Text style={styles.name}>{log.name ?? 'Untitled log'}</Text>
      <Text style={styles.meta}>
        {formatDate(log.date)} | {log.isCompleted ? 'Completed' : 'In progress'} |{' '}
        {formatDuration(log.duration)}
      </Text>
      <Text style={styles.notes}>{log.notes ?? 'No notes recorded.'}</Text>
      <PrimaryButton
        label="View log"
        onPress={() =>
          router.push({
            pathname: '/(app)/logs/[id]',
            params: { id: log.id },
          } as Href)
        }
        tone="muted"
      />
    </GlowCard>
  );
}

const styles = StyleSheet.create({
  list: {
    gap: tokens.spacing.md,
  },
  name: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 19,
  },
  meta: {
    color: tokens.colors.accentWarm,
    fontFamily: tokens.typography.label,
    fontSize: 13,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  notes: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
});
