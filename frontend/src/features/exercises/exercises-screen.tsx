import { useDeferredValue, useMemo, useState } from 'react';
import { FlatList, StyleSheet, Text } from 'react-native';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import type { Exercise } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { TextField } from '@/components/ui/text-field';
import { tokens } from '@/theme/tokens';

export function ExercisesScreen() {
  const [search, setSearch] = useState('');
  const deferredSearch = useDeferredValue(search);
  const exercisesQuery = useQuery({
    queryKey: ['exercises', 'catalogue', 1, 20],
    queryFn: () => apiClient.getExercises(),
  });

  const items = useMemo(() => {
    const query = deferredSearch.trim().toLowerCase();
    const source = exercisesQuery.data?.items ?? [];

    if (!query) {
      return source;
    }

    return source.filter((exercise) => {
      return [exercise.name, exercise.bodyPart, exercise.targetMuscle]
        .filter(Boolean)
        .some((value) => value.toLowerCase().includes(query));
    });
  }, [deferredSearch, exercisesQuery.data?.items]);

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Catalogue"
        title="Exercises"
        subtitle="Server state is cached with TanStack Query and filtered client-side with deferred search so tablet and desktop layouts stay responsive while you type."
      />

      <GlowCard>
        <TextField
          label="Search"
          value={search}
          onChangeText={setSearch}
          placeholder="Chest, squat, hamstrings..."
        />
      </GlowCard>

      <FlatList
        data={items}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        renderItem={({ item }) => <ExerciseCard exercise={item} />}
        scrollEnabled={false}
      />
    </AppScreen>
  );
}

function ExerciseCard({ exercise }: { exercise: Exercise }) {
  return (
    <GlowCard>
      <Text style={styles.exerciseName}>{exercise.name}</Text>
      <Text style={styles.exerciseMeta}>
        {exercise.bodyPart} · {exercise.targetMuscle}
      </Text>
      <Text style={styles.exerciseBody}>
        {exercise.description ?? 'No description has been synced for this exercise yet.'}
      </Text>
    </GlowCard>
  );
}

const styles = StyleSheet.create({
  list: {
    gap: tokens.spacing.md,
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
