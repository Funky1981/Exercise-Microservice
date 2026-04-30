import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useDeferredValue, useMemo, useState, type PropsWithChildren } from 'react';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import { Image } from 'expo-image';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise, ExerciseFilters } from '@/api/types';
import { FilterChip } from '@/components/ui/filter-chip';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { getPlayableExercisePreviewUrl, hasPlayableExerciseMedia } from '@/features/exercises/exercise-media';
import { pickResponsiveValue, useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

const PAGE_SIZE = 12;

export function ExercisesScreen() {
  const { breakpoint } = useBreakpoint();
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);
  const [mediaOnly, setMediaOnly] = useState(false);
  const numColumns = pickResponsiveValue(breakpoint, {
    compact: 1,
    medium: 2,
    expanded: 3,
  });
  const deferredSearch = useDeferredValue(search);
  const filters = useMemo<ExerciseFilters>(() => ({ mediaOnly }), [mediaOnly]);
  const exercisesQuery = useQuery({
    queryKey: queryKeys.exercises.catalogue(pageNumber, PAGE_SIZE, mediaOnly ? { mediaOnly: 'true' } : {}),
    queryFn: () => apiClient.getExercises(pageNumber, PAGE_SIZE, filters),
    placeholderData: keepPreviousData,
  });

  const items = useMemo(() => {
    const query = deferredSearch.trim().toLowerCase();
    const source = (exercisesQuery.data?.items ?? []).filter((exercise) =>
      mediaOnly ? hasPlayableExerciseMedia(exercise) : true
    );

    if (!query) {
      return source;
    }

    return source.filter((exercise) => {
      return [exercise.name, exercise.bodyPart, exercise.targetMuscle, exercise.equipment ?? '']
        .some((value) => value.toLowerCase().includes(query));
    });
  }, [deferredSearch, exercisesQuery.data?.items, mediaOnly]);

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
        <View style={styles.filterRow}>
          <FilterChip
            label="All exercises"
            selected={!mediaOnly}
            onPress={() => {
              setMediaOnly(false);
              setPageNumber(1);
            }}
          />
          <FilterChip
            label="Media only"
            selected={mediaOnly}
            onPress={() => {
              setMediaOnly((current) => !current);
              setPageNumber(1);
            }}
          />
        </View>
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
  const previewUrl = getPlayableExercisePreviewUrl(exercise);
  const hasMedia = hasPlayableExerciseMedia(exercise);

  return (
    <GlowCard>
      {previewUrl ? (
        <Image contentFit="cover" source={{ uri: previewUrl }} style={styles.exercisePreview} />
      ) : (
        <View style={[styles.exercisePreview, styles.exercisePreviewPlaceholder]}>
          <Text style={styles.exercisePreviewPlaceholderText}>No media</Text>
        </View>
      )}
      <Text style={styles.exerciseName}>{exercise.name}</Text>
      <Text style={styles.exerciseMeta}>
        {exercise.bodyPart} | {exercise.targetMuscle}
      </Text>
      <Text style={styles.exerciseSupportText}>
        {exercise.difficulty ?? 'Difficulty not set'}
        {' · '}
        {exercise.category ?? 'Category not set'}
      </Text>
      <Text style={styles.exerciseSupportText}>
        {hasMedia
          ? `Video available${exercise.mediaSourceProvider ? ` · ${exercise.mediaSourceProvider}` : ''}`
          : 'No verified example video available yet'}
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
  filterRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  columnWrapper: {
    gap: tokens.spacing.md,
  },
  column: {
    flex: 1,
  },
  exercisePreview: {
    width: '100%',
    height: 160,
    borderRadius: tokens.radius.lg,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  exercisePreviewPlaceholder: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  exercisePreviewPlaceholderText: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    letterSpacing: 1,
    textTransform: 'uppercase',
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
  exerciseSupportText: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
  },
});
