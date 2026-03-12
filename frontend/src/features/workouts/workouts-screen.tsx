import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { FlatList, StyleSheet, Text } from 'react-native';
import { router, type Href } from 'expo-router';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Workout } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { formatDate, formatDuration } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

const PAGE_SIZE = 10;

export function WorkoutsScreen() {
  const { session } = useSession();
  const [pageNumber, setPageNumber] = useState(1);
  const workoutsQuery = useQuery({
    queryKey: queryKeys.workouts.list(session?.userId, pageNumber, PAGE_SIZE),
    queryFn: () => apiClient.getWorkouts(pageNumber, PAGE_SIZE),
    enabled: Boolean(session?.userId),
    placeholderData: keepPreviousData,
  });

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Sessions"
        title="Workouts"
        subtitle="Workout pages stay stable during transitions with placeholder data, while detail routes handle completion, editing, deletion, and exercise association management."
      />

      <PrimaryButton
        label="Create workout"
        onPress={() => router.push('/(app)/workouts/new' as Href)}
      />

      {workoutsQuery.isPending ? (
        <StatusCard title="Loading workouts" body="Fetching the latest workout page." busy />
      ) : workoutsQuery.isError ? (
        <StatusCard
          title="Unable to load workouts"
          body={workoutsQuery.error instanceof Error ? workoutsQuery.error.message : 'Try again in a moment.'}
        />
      ) : (
        <>
          <FlatList
            data={workoutsQuery.data?.items ?? []}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            renderItem={({ item }) => <WorkoutCard workout={item} />}
            ListEmptyComponent={
              <StatusCard
                title="No workouts scheduled"
                body="Create your first workout and it will show up here."
              />
            }
            scrollEnabled={false}
          />
          <PaginationControls
            pageNumber={workoutsQuery.data?.pageNumber ?? pageNumber}
            totalPages={workoutsQuery.data?.totalPages ?? 1}
            totalCount={workoutsQuery.data?.totalCount ?? 0}
            busy={workoutsQuery.isFetching}
            onPageChange={setPageNumber}
          />
        </>
      )}
    </AppScreen>
  );
}

function WorkoutCard({ workout }: { workout: Workout }) {
  return (
    <GlowCard>
      <Text style={styles.name}>{workout.name ?? 'Untitled workout'}</Text>
      <Text style={styles.meta}>
        {formatDate(workout.date)} | {workout.isCompleted ? 'Completed' : 'Scheduled'} |{' '}
        {formatDuration(workout.duration)}
      </Text>
      <Text style={styles.notes}>{workout.notes ?? 'No notes recorded.'}</Text>
      <Text style={styles.metaSecondary}>{workout.exercises.length} linked exercises</Text>
      <PrimaryButton
        label="View workout"
        onPress={() =>
          router.push({
            pathname: '/(app)/workouts/[id]',
            params: { id: workout.id },
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
