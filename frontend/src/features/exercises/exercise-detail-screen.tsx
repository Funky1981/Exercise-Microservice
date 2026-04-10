import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type ExerciseDetailScreenProps = {
  exerciseId?: string;
};

export function ExerciseDetailScreen({ exerciseId }: ExerciseDetailScreenProps) {
  const { isCompact } = useBreakpoint();
  const { session } = useSession();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [selectedWorkoutId, setSelectedWorkoutId] = useState<string | null>(null);

  const exerciseQuery = useQuery({
    queryKey: exerciseId ? queryKeys.exercises.detail(exerciseId) : ['exercises', 'detail', 'missing'],
    queryFn: () => apiClient.getExerciseById(exerciseId!),
    enabled: Boolean(exerciseId),
  });

  const workoutsQuery = useQuery({
    queryKey: queryKeys.workouts.list(session?.userId, 1, 100),
    queryFn: () => apiClient.getWorkouts(1, 100),
    enabled: Boolean(session) && Boolean(exerciseId),
  });

  const addToWorkoutMutation = useMutation({
    mutationFn: (workoutId: string) =>
      apiClient.addWorkoutExercise(workoutId, { exerciseId: exerciseId! }),
    onSuccess: async () => {
      if (selectedWorkoutId) {
        await queryClient.invalidateQueries({
          queryKey: queryKeys.workouts.detail(session?.userId, selectedWorkoutId),
        });
      }
      showToast({ tone: 'success', title: 'Exercise added to workout' });
      setSelectedWorkoutId(null);
    },
    onError: (err) => {
      showToast({
        tone: 'error',
        title: 'Failed to add',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
  });

  const quickLogMutation = useMutation({
    mutationFn: async () => {
      const exercise = exerciseQuery.data;
      if (!exercise) throw new Error('Exercise not loaded');
      const log = await apiClient.createExerciseLog({
        name: `Quick log: ${exercise.name}`,
        date: new Date().toISOString(),
      });
      await apiClient.addExerciseLogEntry(log.id, {
        exerciseId: exercise.id,
        sets: 1,
        reps: 0,
      });
      return log.id;
    },
    onSuccess: (logId) => {
      showToast({ tone: 'success', title: 'Log created' });
      router.push({
        pathname: '/(app)/logs/[id]',
        params: { id: logId },
      } as Href);
    },
    onError: (err) => {
      showToast({
        tone: 'error',
        title: 'Failed to create log',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
  });

  if (!exerciseId) {
    return (
      <AppScreen>
        <StatusCard
          title="Exercise not found"
          body="This route was opened without an exercise id."
        />
      </AppScreen>
    );
  }

  if (exerciseQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard
          title="Loading exercise"
          body="Pulling the latest detail from the exercise catalogue."
          busy
        />
      </AppScreen>
    );
  }

  if (exerciseQuery.isError || !exerciseQuery.data) {
    return (
      <AppScreen>
        <StatusCard
          title="Unable to load exercise"
          body={exerciseQuery.error instanceof Error ? exerciseQuery.error.message : 'Try again in a moment.'}
        />
      </AppScreen>
    );
  }

  const exercise = exerciseQuery.data;

  // Available workouts that don't already contain this exercise
  const availableWorkouts = (workoutsQuery.data?.items ?? []).filter(
    (w) => !w.isCompleted && !w.exercises.some((e) => e.id === exercise.id)
  );

  return (
    <AppScreen>
      <SectionHeading
        eyebrow={exercise.bodyPart}
        title={exercise.name}
        subtitle={`${exercise.targetMuscle} · ${exercise.equipment ?? 'Bodyweight'}`}
      />

      <View style={[styles.metaGrid, !isCompact && styles.metaGridWide]}>
        <GlowCard style={styles.metaCard}>
          <Text style={styles.metaLabel}>Target muscle</Text>
          <Text style={styles.metaValue}>{exercise.targetMuscle}</Text>
        </GlowCard>
        <GlowCard style={styles.metaCard}>
          <Text style={styles.metaLabel}>Equipment</Text>
          <Text style={styles.metaValue}>{exercise.equipment ?? 'Bodyweight / unspecified'}</Text>
        </GlowCard>
        <GlowCard style={styles.metaCard}>
          <Text style={styles.metaLabel}>Difficulty</Text>
          <Text style={styles.metaValue}>{exercise.difficulty ?? 'Not set'}</Text>
        </GlowCard>
      </View>

      <GlowCard>
        <Text style={styles.sectionTitle}>Description</Text>
        <Text style={styles.body}>
          {exercise.description ?? 'No description is stored for this exercise yet.'}
        </Text>
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Actions</Text>
        <View style={styles.actionsRow}>
          <PrimaryButton
            label="Quick log this exercise"
            onPress={() => quickLogMutation.mutate()}
            busy={quickLogMutation.isPending}
            style={styles.actionBtn}
          />
          <PrimaryButton
            label="Create workout with this"
            onPress={() =>
              router.push({
                pathname: '/(app)/workouts/new',
                params: { prefillExerciseId: exercise.id },
              } as Href)
            }
            tone="muted"
            style={styles.actionBtn}
          />
          <PrimaryButton
            label="View trends"
            onPress={() =>
              router.push(`/(app)/analytics/${exercise.id}` as Href)
            }
            tone="muted"
            style={styles.actionBtn}
          />
        </View>
      </GlowCard>

      {availableWorkouts.length > 0 ? (
        <GlowCard>
          <Text style={styles.sectionTitle}>Add to existing workout</Text>
          <View style={styles.workoutList}>
            {availableWorkouts.slice(0, 5).map((w) => (
              <PrimaryButton
                key={w.id}
                label={w.name ?? 'Untitled workout'}
                onPress={() => {
                  setSelectedWorkoutId(w.id);
                  addToWorkoutMutation.mutate(w.id);
                }}
                tone="muted"
                busy={addToWorkoutMutation.isPending && selectedWorkoutId === w.id}
                disabled={addToWorkoutMutation.isPending}
              />
            ))}
          </View>
        </GlowCard>
      ) : null}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  metaGrid: {
    gap: tokens.spacing.md,
  },
  metaGridWide: {
    flexDirection: 'row',
  },
  metaCard: {
    gap: tokens.spacing.sm,
    flex: 1,
  },
  metaLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  metaValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  sectionTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  body: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 24,
  },
  actionsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionBtn: {
    flexGrow: 1,
    minWidth: 180,
  },
  workoutList: {
    gap: tokens.spacing.sm,
  },
});
