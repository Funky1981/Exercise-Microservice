import { FlatList, StyleSheet, Text } from 'react-native';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import type { Workout } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { tokens } from '@/theme/tokens';

export function WorkoutsScreen() {
  const workoutsQuery = useQuery({
    queryKey: ['workouts', 'list', 1, 20],
    queryFn: () => apiClient.getWorkouts(),
  });

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Sessions"
        title="Workouts"
        subtitle="This screen is wired to the live paged workout endpoint. The next phase can layer mutations and optimistic updates on top of the existing query cache."
      />

      <FlatList
        data={workoutsQuery.data?.items ?? []}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        renderItem={({ item }) => <WorkoutCard workout={item} />}
        scrollEnabled={false}
      />
    </AppScreen>
  );
}

function WorkoutCard({ workout }: { workout: Workout }) {
  const date = new Date(workout.date);

  return (
    <GlowCard>
      <Text style={styles.name}>{workout.name ?? 'Untitled workout'}</Text>
      <Text style={styles.meta}>
        {date.toLocaleDateString()} · {workout.isCompleted ? 'Completed' : 'Scheduled'}
      </Text>
      <Text style={styles.notes}>{workout.notes ?? 'No notes recorded.'}</Text>
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
