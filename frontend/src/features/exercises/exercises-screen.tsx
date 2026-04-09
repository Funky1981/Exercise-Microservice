import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useDeferredValue, useMemo, useState, type PropsWithChildren } from 'react';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { pickResponsiveValue, useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

const PAGE_SIZE = 12;

export function ExercisesScreen() {
  const { breakpoint } = useBreakpoint();
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const numColumns = pickResponsiveValue(breakpoint, {
    compact: 1,
    medium: 2,
    expanded: 3,
  });
  const deferredSearch = useDeferredValue(search);
  const exercisesQuery = useQuery({
    queryKey: queryKeys.exercises.catalogue(pageNumber, PAGE_SIZE),
    queryFn: () => apiClient.getExercises(pageNumber, PAGE_SIZE),
    placeholderData: keepPreviousData,
  });

  const items = useMemo(() => {
    const query = deferredSearch.trim().toLowerCase();
    const source = exercisesQuery.data?.items ?? [];

    if (!query) {
      return source;
    }

    return source.filter((exercise) => {
      return [exercise.name, exercise.bodyPart, exercise.targetMuscle, exercise.equipment ?? '']
        .some((value) => value.toLowerCase().includes(query));
    });
  }, [deferredSearch, exercisesQuery.data?.items]);

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Catalogue"
        title="Exercises"
        subtitle="Server state is paged and cached with TanStack Query, while client-side deferred search keeps the current page responsive as you type."
      />

      <GlowCard>
        <TextField
          label="Search"
          value={search}
          onChangeText={setSearch}
          placeholder="Chest, squat, hamstrings..."
          helperText="Search filters the current page. Use pagination to move through the full catalogue."
        />
      </GlowCard>

      {exercisesQuery.isPending ? (
        <StatusCard
          title="Loading exercises"
          body="Fetching the current catalogue page from the backend."
          busy
        />
      ) : exercisesQuery.isError ? (
        <StatusCard
          title="Unable to load exercises"
          body={exercisesQuery.error instanceof Error ? exercisesQuery.error.message : 'Try again in a moment.'}
        />
      ) : (
        <>
          <FlatList
            key={`exercises-${numColumns}`}
            data={items}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            columnWrapperStyle={numColumns > 1 ? styles.columnWrapper : undefined}
            numColumns={numColumns}
            renderItem={({ item }) => (
              <ExerciseGridItem columns={numColumns}>
                <ExerciseCard exercise={item} />
              </ExerciseGridItem>
            )}
            ListEmptyComponent={
              <StatusCard
                title="No exercises matched"
                body="Try a broader search term or move to another page."
              />
            }
            scrollEnabled={false}
          />
          <PaginationControls
            pageNumber={exercisesQuery.data?.pageNumber ?? pageNumber}
            totalPages={exercisesQuery.data?.totalPages ?? 1}
            totalCount={exercisesQuery.data?.totalCount ?? 0}
            busy={exercisesQuery.isFetching}
            onPageChange={setPageNumber}
          />
        </>
      )}
    </AppScreen>
  );
}

function ExerciseCard({ exercise }: { exercise: Exercise }) {
  return (
    <GlowCard>
      <Text style={styles.exerciseName}>{exercise.name}</Text>
      <Text style={styles.exerciseMeta}>
        {exercise.bodyPart} | {exercise.targetMuscle}
      </Text>
      <Text style={styles.exerciseBody}>
        {exercise.description ?? 'No description has been synced for this exercise yet.'}
      </Text>
      <PrimaryButton
        label="View detail"
        onPress={() =>
          router.push({
            pathname: '/(app)/exercises/[id]',
            params: { id: exercise.id },
          } as Href)
        }
        tone="muted"
      />
    </GlowCard>
  );
}

function ExerciseGridItem({
  children,
  columns,
}: PropsWithChildren<{
  columns: number;
}>) {
  return <View style={columns > 1 ? styles.column : undefined}>{children}</View>;
}

const styles = StyleSheet.create({
  list: {
    gap: tokens.spacing.md,
  },
  columnWrapper: {
    gap: tokens.spacing.md,
  },
  column: {
    flex: 1,
  },
  exerciseName: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  exerciseMeta: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.label,
    fontSize: 13,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  exerciseBody: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
});
