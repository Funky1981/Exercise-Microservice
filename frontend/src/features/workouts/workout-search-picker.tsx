import { useDeferredValue, useMemo, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Workout } from '@/api/types';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { TextField } from '@/components/ui/text-field';
import { formatDate } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutSearchPickerProps = {
  label: string;
  buttonLabel: string;
  excludedWorkoutIds?: string[];
  disabled?: boolean;
  onAdd: (workout: Workout) => void;
};

export function WorkoutSearchPicker({
  label,
  buttonLabel,
  excludedWorkoutIds = [],
  disabled = false,
  onAdd,
}: WorkoutSearchPickerProps) {
  const { session } = useSession();
  const [search, setSearch] = useState('');
  const [selectedWorkout, setSelectedWorkout] = useState<Workout | null>(null);
  const deferredSearch = useDeferredValue(search);

  const workoutsQuery = useQuery({
    queryKey: queryKeys.workouts.list(session?.userId, 1, 100),
    queryFn: () => apiClient.getWorkouts(1, 100),
    enabled: Boolean(session?.userId),
  });

  const items = useMemo(() => {
    const query = deferredSearch.trim().toLowerCase();
    const excluded = new Set(excludedWorkoutIds);
    const source = (workoutsQuery.data?.items ?? []).filter((item) => !excluded.has(item.id));

    if (!query) {
      return source.slice(0, 8);
    }

    return source
      .filter((workout) =>
        [workout.name ?? '', workout.notes ?? ''].some((value) =>
          value.toLowerCase().includes(query)
        )
      )
      .slice(0, 8);
  }, [deferredSearch, excludedWorkoutIds, workoutsQuery.data?.items]);

  return (
    <View style={styles.wrapper}>
      <TextField
        label={label}
        value={search}
        onChangeText={setSearch}
        placeholder="Search by workout name or notes"
      />
      <View style={styles.results}>
        {items.map((workout) => (
          <Pressable
            key={workout.id}
            onPress={() => setSelectedWorkout(workout)}
            style={[
              styles.resultCard,
              selectedWorkout?.id === workout.id && styles.resultCardSelected,
            ]}>
            <Text style={styles.resultTitle}>{workout.name ?? 'Untitled workout'}</Text>
            <Text style={styles.resultMeta}>
              {formatDate(workout.date)} | {workout.isCompleted ? 'Completed' : 'Scheduled'}
            </Text>
            <Text style={styles.resultBody}>{workout.notes ?? 'No notes recorded.'}</Text>
          </Pressable>
        ))}
      </View>
      {selectedWorkout ? (
        <GlowCard style={styles.selectionCard}>
          <Text style={styles.selectionLabel}>Selected workout</Text>
          <Text style={styles.selectionTitle}>{selectedWorkout.name ?? 'Untitled workout'}</Text>
          <Text style={styles.selectionMeta}>{formatDate(selectedWorkout.date)}</Text>
          <PrimaryButton
            label={buttonLabel}
            onPress={() => {
              onAdd(selectedWorkout);
              setSelectedWorkout(null);
              setSearch('');
            }}
            disabled={disabled}
          />
        </GlowCard>
      ) : null}
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: {
    gap: tokens.spacing.md,
  },
  results: {
    gap: tokens.spacing.sm,
  },
  resultCard: {
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    borderColor: tokens.colors.borderSoft,
    backgroundColor: tokens.colors.surfaceRaised,
    padding: tokens.spacing.md,
    gap: tokens.spacing.xs,
  },
  resultCardSelected: {
    borderColor: tokens.colors.accent,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  resultTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 15,
  },
  resultMeta: {
    color: tokens.colors.accentWarm,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  resultBody: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  selectionCard: {
    padding: tokens.spacing.md,
  },
  selectionLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  selectionTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  selectionMeta: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
});
