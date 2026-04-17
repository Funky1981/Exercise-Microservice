import { useCallback, useDeferredValue, useEffect, useMemo, useRef, useState } from 'react';
import {
  ActivityIndicator,
  FlatList,
  Platform,
  Pressable,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { Image } from 'expo-image';
import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise, ExerciseFilters, ExerciseRegion } from '@/api/types';
import { FilterChip } from '@/components/ui/filter-chip';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import {
  getExercisePreferences,
  recordRecentExercise,
  toggleFavouriteExercise,
} from '@/features/exercises/exercise-preferences';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

const PAGE_SIZE = 24;

const REGION_LABELS: Record<ExerciseRegion, string> = {
  'upper-body': 'Upper body',
  'lower-body': 'Lower body',
  core: 'Core',
  cardio: 'Cardio',
  other: 'Other',
};

const REGION_DESCRIPTIONS: Record<ExerciseRegion, string> = {
  'upper-body': 'Back, chest, shoulders, arms',
  'lower-body': 'Quads, hamstrings, glutes, calves',
  core: 'Abs, obliques, lower back',
  cardio: 'Running, cycling, HIIT',
  other: 'Full-body & unconventional',
};

type ExerciseCataloguePickerProps = {
  title?: string;
  actionLabel?: string;
  disabled?: boolean;
  excludedExerciseIds?: string[];
  selectedExerciseIds?: string[];
  selectionMode?: 'single' | 'multi';
  onAdd?: (exercise: Exercise) => void;
  onToggle?: (exercise: Exercise) => void;
};

export function ExerciseCataloguePicker({
  title = 'Choose exercises',
  actionLabel = 'Add exercise',
  disabled = false,
  excludedExerciseIds = [],
  selectedExerciseIds = [],
  selectionMode = 'single',
  onAdd,
  onToggle,
}: ExerciseCataloguePickerProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const [region, setRegion] = useState<ExerciseRegion | ''>('');
  const [bodyPart, setBodyPart] = useState('');
  const [equipment, setEquipment] = useState('');
  const [search, setSearch] = useState('');
  const deferredSearch = useDeferredValue(search.trim());
  const hasFilters = Boolean(region || bodyPart || equipment || deferredSearch);
  const listRef = useRef<FlatList>(null);

  const filtersQuery = useQuery({
    queryKey: queryKeys.exercises.filters(),
    queryFn: () => apiClient.getExerciseFilters(),
  });

  const preferencesQuery = useQuery({
    queryKey: ['exercise-preferences', session?.userId],
    queryFn: () => getExercisePreferences(session!.userId),
    enabled: Boolean(session?.userId),
  });

  const filters = useMemo<ExerciseFilters>(
    () => ({
      region: region || null,
      bodyPart: bodyPart || null,
      equipment: equipment || null,
      search: deferredSearch || null,
    }),
    [bodyPart, deferredSearch, equipment, region]
  );

  useEffect(() => {
    if (!region) {
      setBodyPart('');
      return;
    }

    const validBodyParts = filtersQuery.data?.bodyPartsByRegion[region] ?? [];
    if (bodyPart && !validBodyParts.includes(bodyPart)) {
      setBodyPart('');
    }
  }, [bodyPart, filtersQuery.data?.bodyPartsByRegion, region]);

  const catalogueQuery = useInfiniteQuery({
    queryKey: ['exercises', 'catalogue-infinite', filters],
    queryFn: ({ pageParam }) => apiClient.getExercises(pageParam, PAGE_SIZE, filters),
    initialPageParam: 1,
    getNextPageParam: (lastPage) =>
      lastPage.hasNextPage ? lastPage.pageNumber + 1 : undefined,
    enabled: hasFilters,
  });

  const favouriteMutation = useMutation({
    mutationFn: async (exerciseId: string) => {
      if (!session?.userId) {
        return preferencesQuery.data;
      }

      return toggleFavouriteExercise(session.userId, exerciseId);
    },
    onSuccess: (nextPreferences) => {
      if (!session?.userId || !nextPreferences) {
        return;
      }

      queryClient.setQueryData(['exercise-preferences', session.userId], nextPreferences);
    },
  });

  const items = useMemo(() => {
    const excluded = new Set(excludedExerciseIds);

    return (catalogueQuery.data?.pages ?? [])
      .flatMap((page) => page.items)
      .filter((exercise) => !excluded.has(exercise.id));
  }, [catalogueQuery.data?.pages, excludedExerciseIds]);

  const totalCount = catalogueQuery.data?.pages[0]?.totalCount ?? 0;

  const visibleMap = useMemo(
    () => new Map(items.map((exercise) => [exercise.id, exercise])),
    [items]
  );

  const recentExercises = useMemo(() => {
    return (preferencesQuery.data?.recentExerciseIds ?? [])
      .map((id) => visibleMap.get(id))
      .filter((exercise): exercise is Exercise => Boolean(exercise));
  }, [preferencesQuery.data?.recentExerciseIds, visibleMap]);

  const favouriteExercises = useMemo(() => {
    return (preferencesQuery.data?.favouriteExerciseIds ?? [])
      .map((id) => visibleMap.get(id))
      .filter((exercise): exercise is Exercise => Boolean(exercise));
  }, [preferencesQuery.data?.favouriteExerciseIds, visibleMap]);

  const bodyPartOptions = useMemo(() => {
    return (region ? filtersQuery.data?.bodyPartsByRegion[region] : []) ?? [];
  }, [filtersQuery.data?.bodyPartsByRegion, region]);

  const equipmentOptions = useMemo(() => {
    return filtersQuery.data?.equipment ?? [];
  }, [filtersQuery.data?.equipment]);

  async function markRecent(exerciseId: string) {
    if (!session?.userId) {
      return;
    }

    const nextPreferences = await recordRecentExercise(session.userId, exerciseId);
    queryClient.setQueryData(['exercise-preferences', session.userId], nextPreferences);
  }

  function handleExerciseAction(exercise: Exercise) {
    void markRecent(exercise.id);

    if (selectionMode === 'multi') {
      onToggle?.(exercise);
      return;
    }

    onAdd?.(exercise);
  }

  function clearAllFilters() {
    setRegion('');
    setBodyPart('');
    setEquipment('');
    setSearch('');
  }

  const handleEndReached = useCallback(() => {
    if (catalogueQuery.hasNextPage && !catalogueQuery.isFetchingNextPage) {
      void catalogueQuery.fetchNextPage();
    }
  }, [catalogueQuery]);

  const selectedSet = useMemo(() => new Set(selectedExerciseIds), [selectedExerciseIds]);
  const favouriteSet = useMemo(
    () => new Set(preferencesQuery.data?.favouriteExerciseIds ?? []),
    [preferencesQuery.data?.favouriteExerciseIds]
  );

  // ── Render ──────────────────────────────────────────────────────

  return (
    <View style={styles.wrapper}>
      {/* ── Search bar (top priority) ── */}
      <GlowCard>
        <Text style={styles.title}>{title}</Text>
        <TextField
          label="Search exercises"
          value={search}
          onChangeText={setSearch}
          placeholder="Type a name, muscle, or equipment..."
        />
        {hasFilters && !catalogueQuery.isPending ? (
          <Text style={styles.resultCount}>
            Showing {items.length} of {totalCount} exercises
          </Text>
        ) : null}
      </GlowCard>

      {/* ── Region selector ── */}
      <GlowCard>
        <Text style={styles.sectionLabel}>Pick a region</Text>
        <View style={styles.regionGrid}>
          {(filtersQuery.data?.regions ?? []).map((entry) => (
            <RegionCard
              key={entry}
              region={entry}
              label={REGION_LABELS[entry]}
              description={REGION_DESCRIPTIONS[entry]}
              selected={region === entry}
              onPress={() => {
                setRegion(region === entry ? '' : entry);
                if (region === entry) setBodyPart('');
              }}
            />
          ))}
        </View>
      </GlowCard>

      {/* ── Body part chips (visible when region is selected) ── */}
      {region && bodyPartOptions.length > 0 ? (
        <GlowCard>
          <Text style={styles.sectionLabel}>Body part</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            <View style={styles.chipRow}>
              <FilterChip
                label="All"
                selected={!bodyPart}
                onPress={() => setBodyPart('')}
              />
              {bodyPartOptions.map((part) => (
                <FilterChip
                  key={part}
                  label={part}
                  selected={bodyPart === part}
                  onPress={() => setBodyPart(bodyPart === part ? '' : part)}
                />
              ))}
            </View>
          </ScrollView>
        </GlowCard>
      ) : null}

      {/* ── Equipment chips ── */}
      {equipmentOptions.length > 0 ? (
        <GlowCard>
          <Text style={styles.sectionLabel}>Equipment</Text>
          <ScrollView horizontal showsHorizontalScrollIndicator={false}>
            <View style={styles.chipRow}>
              <FilterChip
                label="All"
                selected={!equipment}
                onPress={() => setEquipment('')}
              />
              {equipmentOptions.map((eq) => (
                <FilterChip
                  key={eq}
                  label={eq}
                  selected={equipment === eq}
                  onPress={() => setEquipment(equipment === eq ? '' : eq)}
                />
              ))}
            </View>
          </ScrollView>
        </GlowCard>
      ) : null}

      {/* ── Active filter summary ── */}
      {hasFilters ? (
        <View style={styles.activeFilters}>
          {region ? (
            <FilterChip
              label={REGION_LABELS[region]}
              selected
              onPress={() => { setRegion(''); setBodyPart(''); }}
            />
          ) : null}
          {bodyPart ? (
            <FilterChip label={bodyPart} selected onPress={() => setBodyPart('')} />
          ) : null}
          {equipment ? (
            <FilterChip label={equipment} selected onPress={() => setEquipment('')} />
          ) : null}
          {deferredSearch ? (
            <FilterChip label={`"${deferredSearch}"`} selected onPress={() => setSearch('')} />
          ) : null}
          <PrimaryButton label="Clear all" onPress={clearAllFilters} tone="muted" />
        </View>
      ) : null}

      {/* ── Preferences (shown when no filters active) ── */}
      {!hasFilters ? (
        <>
          {recentExercises.length > 0 ? (
            <GlowCard>
              <Text style={styles.sectionLabel}>Recent exercises</Text>
              {recentExercises.map((exercise) => (
                <CompactExerciseRow
                  key={`recent-${exercise.id}`}
                  exercise={exercise}
                  selected={selectedSet.has(exercise.id)}
                  favourite={favouriteSet.has(exercise.id)}
                  selectionMode={selectionMode}
                  disabled={disabled}
                  onAction={() => handleExerciseAction(exercise)}
                  onToggleFavourite={() => favouriteMutation.mutate(exercise.id)}
                />
              ))}
            </GlowCard>
          ) : null}

          {favouriteExercises.length > 0 ? (
            <GlowCard>
              <Text style={styles.sectionLabel}>Favourites</Text>
              {favouriteExercises.map((exercise) => (
                <CompactExerciseRow
                  key={`fav-${exercise.id}`}
                  exercise={exercise}
                  selected={selectedSet.has(exercise.id)}
                  favourite={favouriteSet.has(exercise.id)}
                  selectionMode={selectionMode}
                  disabled={disabled}
                  onAction={() => handleExerciseAction(exercise)}
                  onToggleFavourite={() => favouriteMutation.mutate(exercise.id)}
                />
              ))}
            </GlowCard>
          ) : null}
        </>
      ) : null}

      {/* ── Results ── */}
      {hasFilters && catalogueQuery.isPending ? (
        <View style={styles.skeletonList}>
          {[1, 2, 3, 4].map((item) => (
            <View key={item} style={styles.skeletonRow}>
              <View style={styles.skeletonBlock} />
            </View>
          ))}
        </View>
      ) : hasFilters && catalogueQuery.isError ? (
        <StatusCard
          title="Unable to load exercises"
          body={catalogueQuery.error instanceof Error ? catalogueQuery.error.message : 'Try again in a moment.'}
        />
      ) : hasFilters && items.length === 0 ? (
        <StatusCard
          title="No exercises matched"
          body="Try removing a filter or broadening your search."
        />
      ) : hasFilters ? (
        <FlatList
          ref={listRef}
          data={items}
          keyExtractor={(exercise) => exercise.id}
          scrollEnabled={false}
          onEndReached={handleEndReached}
          onEndReachedThreshold={0.4}
          renderItem={({ item: exercise }) => (
            <CompactExerciseRow
              exercise={exercise}
              selected={selectedSet.has(exercise.id)}
              favourite={favouriteSet.has(exercise.id)}
              selectionMode={selectionMode}
              disabled={disabled}
              onAction={() => handleExerciseAction(exercise)}
              onToggleFavourite={() => favouriteMutation.mutate(exercise.id)}
            />
          )}
          ItemSeparatorComponent={ListSeparator}
          ListFooterComponent={
            catalogueQuery.isFetchingNextPage ? (
              <View style={styles.loadingFooter}>
                <ActivityIndicator color={tokens.colors.accent} />
                <Text style={styles.loadingText}>Loading more...</Text>
              </View>
            ) : null
          }
        />
      ) : null}
    </View>
  );
}

// ── Region Card ───────────────────────────────────────────────────

type RegionCardProps = {
  region: ExerciseRegion;
  label: string;
  description: string;
  selected: boolean;
  onPress: () => void;
};

function RegionCard({ label, description, selected, onPress }: RegionCardProps) {
  const [hovered, setHovered] = useState(false);

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      onPress={onPress}
      onHoverIn={() => setHovered(true)}
      onHoverOut={() => setHovered(false)}
      style={({ pressed }) => [
        styles.regionCard,
        selected && styles.regionCardSelected,
        hovered && !selected && styles.regionCardHovered,
        pressed && styles.regionCardPressed,
      ]}>
      <Text style={[styles.regionLabel, selected && styles.regionLabelSelected]}>{label}</Text>
      <Text style={styles.regionDescription}>{description}</Text>
    </Pressable>
  );
}

// ── Compact Exercise Row ──────────────────────────────────────────

type CompactExerciseRowProps = {
  exercise: Exercise;
  selected: boolean;
  favourite: boolean;
  selectionMode: 'single' | 'multi';
  disabled: boolean;
  onAction: () => void;
  onToggleFavourite: () => void;
};

function CompactExerciseRow({
  exercise,
  selected,
  favourite,
  selectionMode,
  disabled,
  onAction,
  onToggleFavourite,
}: CompactExerciseRowProps) {
  const [hovered, setHovered] = useState(false);
  const isMultiSelected = selectionMode === 'multi' && selected;
  const previewUrl = exercise.mediaKind?.toLowerCase().startsWith('video')
    ? null
    : (exercise.mediaUrl ?? exercise.gifUrl);

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={`${exercise.name} — ${exercise.bodyPart}, ${exercise.targetMuscle}`}
      disabled={disabled || isMultiSelected}
      onPress={onAction}
      onHoverIn={() => setHovered(true)}
      onHoverOut={() => setHovered(false)}
      style={[
        styles.exerciseRow,
        selected && styles.exerciseRowSelected,
        hovered && !selected && styles.exerciseRowHovered,
      ]}>
      {/* Thumbnail */}
      {previewUrl ? (
        <Image contentFit="cover" source={{ uri: previewUrl }} style={styles.thumbnail} />
      ) : (
        <View style={[styles.thumbnail, styles.thumbnailPlaceholder]}>
          <Text style={styles.thumbnailIcon}>💪</Text>
        </View>
      )}

      {/* Info */}
      <View style={styles.exerciseInfo}>
        <Text style={styles.exerciseName} numberOfLines={1}>{exercise.name}</Text>
        <Text style={styles.exerciseMeta} numberOfLines={1}>
          {exercise.bodyPart} · {exercise.targetMuscle}
          {exercise.equipment ? ` · ${exercise.equipment}` : ''}
          {exercise.difficulty ? ` · ${exercise.difficulty}` : ''}
        </Text>
      </View>

      {/* Actions */}
      <View style={styles.exerciseActions}>
        <Pressable
          accessibilityLabel={favourite ? 'Remove favourite' : 'Add favourite'}
          onPress={onToggleFavourite}
          style={[styles.iconButton, favourite && styles.iconButtonActive]}>
          <Text style={styles.iconButtonLabel}>{favourite ? '★' : '☆'}</Text>
        </Pressable>
        <View style={[styles.selectIndicator, selected && styles.selectIndicatorActive]}>
          <Text style={styles.selectIndicatorLabel}>{selected ? '✓' : '+'}</Text>
        </View>
      </View>
    </Pressable>
  );
}

function ListSeparator() {
  return <View style={styles.separator} />;
}

export const ExerciseSearchPicker = ExerciseCataloguePicker;

const styles = StyleSheet.create({
  wrapper: {
    gap: tokens.spacing.md,
  },
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 22,
  },
  sectionLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  resultCount: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 13,
  },
  chipRow: {
    flexDirection: 'row',
    gap: tokens.spacing.sm,
    paddingRight: tokens.spacing.md,
  },
  activeFilters: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
    alignItems: 'center',
  },

  // ── Region grid ──
  regionGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  regionCard: {
    flex: 1,
    minWidth: 140,
    paddingVertical: tokens.spacing.md,
    paddingHorizontal: tokens.spacing.md,
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    borderColor: tokens.colors.borderSoft,
    backgroundColor: tokens.colors.surfaceRaised,
    gap: tokens.spacing.xs,
  },
  regionCardSelected: {
    borderColor: tokens.colors.accent,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  regionCardHovered: {
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  regionCardPressed: {
    opacity: 0.88,
  },
  regionLabel: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 15,
  },
  regionLabelSelected: {
    color: tokens.colors.accent,
  },
  regionDescription: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 12,
    lineHeight: 16,
  },

  // ── Compact exercise row ──
  exerciseRow: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: tokens.spacing.sm,
    paddingVertical: tokens.spacing.sm,
    paddingHorizontal: tokens.spacing.sm,
    borderRadius: tokens.radius.sm,
    borderWidth: 1,
    borderColor: 'transparent',
  },
  exerciseRowSelected: {
    borderColor: tokens.colors.accent,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  exerciseRowHovered: {
    backgroundColor: tokens.colors.surfaceRaised,
    borderColor: tokens.colors.borderSoft,
  },
  thumbnail: {
    width: 48,
    height: 48,
    borderRadius: tokens.radius.sm,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  thumbnailPlaceholder: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  thumbnailIcon: {
    fontSize: 20,
  },
  exerciseInfo: {
    flex: 1,
    gap: 2,
  },
  exerciseName: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 15,
  },
  exerciseMeta: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 12,
  },
  exerciseActions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: tokens.spacing.xs,
  },
  iconButton: {
    width: 36,
    height: 36,
    borderRadius: tokens.radius.pill,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: tokens.colors.surfaceRaised,
  },
  iconButtonActive: {
    backgroundColor: tokens.colors.surfaceStrong,
  },
  iconButtonLabel: {
    fontSize: 18,
    color: tokens.colors.accentWarm,
  },
  selectIndicator: {
    width: 36,
    height: 36,
    borderRadius: tokens.radius.pill,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceRaised,
  },
  selectIndicatorActive: {
    borderColor: tokens.colors.accent,
    backgroundColor: tokens.colors.accent,
  },
  selectIndicatorLabel: {
    fontSize: 16,
    fontFamily: tokens.typography.bodyStrong,
    color: tokens.colors.text,
  },

  // ── Skeleton / Loading ──
  skeletonList: {
    gap: tokens.spacing.sm,
  },
  skeletonRow: {
    height: 64,
    borderRadius: tokens.radius.sm,
    backgroundColor: tokens.colors.surfaceRaised,
    overflow: 'hidden',
  },
  skeletonBlock: {
    flex: 1,
    backgroundColor: tokens.colors.surfaceStrong,
    opacity: 0.4,
  },
  loadingFooter: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    gap: tokens.spacing.sm,
    paddingVertical: tokens.spacing.lg,
  },
  loadingText: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  separator: {
    height: 1,
    backgroundColor: tokens.colors.borderSoft,
    marginHorizontal: tokens.spacing.sm,
  },
});
