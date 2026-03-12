import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { FlatList, StyleSheet, Text } from 'react-native';
import { router, type Href } from 'expo-router';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { ExerciseLog } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { formatDate, formatDuration } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

const PAGE_SIZE = 8;

export function ExerciseLogsScreen() {
  const { session } = useSession();
  const [pageNumber, setPageNumber] = useState(1);
  const logsQuery = useQuery({
    queryKey: queryKeys.exerciseLogs.list(session?.userId, pageNumber, PAGE_SIZE),
    queryFn: () => apiClient.getExerciseLogs(pageNumber, PAGE_SIZE),
    enabled: Boolean(session?.userId),
    placeholderData: keepPreviousData,
  });

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Tracking"
        title="Exercise logs"
        subtitle="Logs hold session-level entries, with paged navigation for history and a richer exercise picker inside each detail screen."
      />

      <PrimaryButton
        label="Create log"
        onPress={() => router.push('/(app)/logs/new' as Href)}
      />

      {logsQuery.isPending ? (
        <StatusCard title="Loading logs" body="Fetching the latest log page." busy />
      ) : logsQuery.isError ? (
        <StatusCard
          title="Unable to load logs"
          body={logsQuery.error instanceof Error ? logsQuery.error.message : 'Try again in a moment.'}
        />
      ) : (
        <>
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
          <PaginationControls
            pageNumber={logsQuery.data?.pageNumber ?? pageNumber}
            totalPages={logsQuery.data?.totalPages ?? 1}
            totalCount={logsQuery.data?.totalCount ?? 0}
            busy={logsQuery.isFetching}
            onPageChange={setPageNumber}
          />
        </>
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
      <Text style={styles.metaSecondary}>{log.entries.length} entries</Text>
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
  metaSecondary: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  notes: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
});
