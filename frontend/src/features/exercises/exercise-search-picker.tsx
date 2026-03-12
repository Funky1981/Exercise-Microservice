import { useDeferredValue, useMemo, useState } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise } from '@/api/types';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { TextField } from '@/components/ui/text-field';
import { tokens } from '@/theme/tokens';

type ExerciseSearchPickerProps = {
  label: string;
  buttonLabel: string;
  disabled?: boolean;
  onAdd: (exercise: Exercise) => void;
};

export function ExerciseSearchPicker({
  label,
  buttonLabel,
  disabled = false,
  onAdd,
}: ExerciseSearchPickerProps) {
  const [search, setSearch] = useState('');
  const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null);
  const deferredSearch = useDeferredValue(search);

  const exercisesQuery = useQuery({
    queryKey: queryKeys.exercises.catalogue(1, 100),
    queryFn: () => apiClient.getExercises(1, 100),
  });

  const items = useMemo(() => {
    const query = deferredSearch.trim().toLowerCase();
    const source = exercisesQuery.data?.items ?? [];

    if (!query) {
      return source.slice(0, 8);
    }

    return source
      .filter((exercise) =>
        [exercise.name, exercise.bodyPart, exercise.targetMuscle, exercise.equipment ?? '']
          .some((value) => value.toLowerCase().includes(query))
      )
      .slice(0, 8);
  }, [deferredSearch, exercisesQuery.data?.items]);

  return (
    <View style={styles.wrapper}>
      <TextField
        label={label}
        value={search}
        onChangeText={setSearch}
        placeholder="Search by name, body part, target muscle, or equipment"
      />
      <View style={styles.results}>
        {items.map((exercise) => (
          <Pressable
            key={exercise.id}
            onPress={() => setSelectedExercise(exercise)}
            style={[
              styles.resultCard,
              selectedExercise?.id === exercise.id && styles.resultCardSelected,
            ]}>
            <Text style={styles.resultTitle}>{exercise.name}</Text>
            <Text style={styles.resultMeta}>
              {exercise.bodyPart} | {exercise.targetMuscle}
            </Text>
            <Text style={styles.resultBody}>
              {exercise.equipment ?? 'Bodyweight / unspecified'}
            </Text>
          </Pressable>
        ))}
      </View>
      {selectedExercise ? (
        <GlowCard style={styles.selectionCard}>
          <Text style={styles.selectionLabel}>Selected exercise</Text>
          <Text style={styles.selectionTitle}>{selectedExercise.name}</Text>
          <Text style={styles.selectionMeta}>
            {selectedExercise.bodyPart} | {selectedExercise.targetMuscle}
          </Text>
          <PrimaryButton
            label={buttonLabel}
            onPress={() => {
              onAdd(selectedExercise);
              setSelectedExercise(null);
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
    color: tokens.colors.accent,
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
