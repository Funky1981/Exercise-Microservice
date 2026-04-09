import { useState } from 'react';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import {
  QueryClient,
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise, WorkoutExercise } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { ExerciseSearchPicker } from '@/features/exercises/exercise-search-picker';
import { formatDuration, formatWorkoutSchedule, minutesToDuration } from '@/lib/format';
import { pickResponsiveValue, useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutDetailScreenProps = {
  workoutId?: string;
};

export function WorkoutDetailScreen({ workoutId }: WorkoutDetailScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const { breakpoint, isCompact } = useBreakpoint();
  const [completionMinutes, setCompletionMinutes] = useState('45');
  const [actionError, setActionError] = useState<string | null>(null);
  const linkedColumns = pickResponsiveValue(breakpoint, {
    compact: 1,
    medium: 2,
    expanded: 2,
  });

  const workoutQuery = useQuery({
    queryKey:
      workoutId && session?.userId
        ? queryKeys.workouts.detail(session.userId, workoutId)
        : ['workouts', 'detail', 'missing'],
    queryFn: () => apiClient.getWorkoutById(workoutId!),
    enabled: Boolean(workoutId) && Boolean(session?.userId),
  });

  const completeMutation = useMutation({
    mutationFn: async () => {
      const minutes = Number(completionMinutes);
      if (Number.isNaN(minutes) || minutes <= 0) {
        throw new Error('Enter a positive number of minutes to complete this workout.');
      }

      return apiClient.completeWorkout(workoutId!, minutesToDuration(minutes));
    },
    onSuccess: async () => {
      setActionError(null);
      await invalidateWorkoutQueries(queryClient, session?.userId, workoutId);
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
      });
      showToast({
        tone: 'success',
        title: 'Workout completed',
        message: 'Analytics and workout detail have been refreshed.',
      });
    },
    onError: (mutationError) => {
      setActionError(
        mutationError instanceof Error
          ? mutationError.message
          : 'Unable to complete workout.'
      );
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => apiClient.deleteWorkout(workoutId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: queryKeys.workouts.list(session?.userId, 1, 10),
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
      });
      showToast({
        tone: 'success',
        title: 'Workout deleted',
      });
      router.replace('/(app)/(tabs)/workouts');
    },
    onError: (mutationError) => {
      setActionError(
        mutationError instanceof Error ? mutationError.message : 'Unable to delete workout.'
      );
    },
  });

  const addExerciseMutation = useMutation({
    mutationFn: (exercise: Exercise) =>
      apiClient.addWorkoutExercise(workoutId!, { exerciseId: exercise.id }),
    onSuccess: async () => {
      await invalidateWorkoutQueries(queryClient, session?.userId, workoutId);
      showToast({
        tone: 'success',
        title: 'Exercise added',
      });
    },
    onError: (mutationError) => {
      setActionError(
        mutationError instanceof Error ? mutationError.message : 'Unable to add exercise.'
      );
    },
  });

  const removeExerciseMutation = useMutation({
    mutationFn: (exerciseId: string) => apiClient.removeWorkoutExercise(workoutId!, exerciseId),
    onSuccess: async () => {
      await invalidateWorkoutQueries(queryClient, session?.userId, workoutId);
      showToast({
        tone: 'success',
        title: 'Exercise removed',
      });
    },
    onError: (mutationError) => {
      setActionError(
        mutationError instanceof Error ? mutationError.message : 'Unable to remove exercise.'
      );
    },
  });

  if (!workoutId) {
    return (
      <AppScreen>
        <StatusCard title="Workout missing" body="This route was opened without a workout id." />
      </AppScreen>
    );
  }

  if (workoutQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading workout" body="Pulling the latest workout detail." busy />
      </AppScreen>
    );
  }

  if (workoutQuery.isError || !workoutQuery.data) {
    return (
      <AppScreen>
        <StatusCard
          title="Unable to load workout"
          body={workoutQuery.error instanceof Error ? workoutQuery.error.message : 'Try again in a moment.'}
        />
      </AppScreen>
    );
  }

  const workout = workoutQuery.data;

  return (
    <AppScreen>
      <SectionHeading
        eyebrow={workout.isCompleted ? 'Completed session' : 'Planned session'}
        title={workout.name ?? 'Untitled workout'}
        subtitle="Workout detail now exposes linked exercises, so this screen can manage the full session composition instead of only session metadata."
      />

      <View style={[styles.detailColumns, !isCompact && styles.detailColumnsWide]}>
        <GlowCard style={styles.primaryColumn}>
          <Text style={styles.label}>Scheduled time</Text>
          <Text style={styles.value}>{formatWorkoutSchedule(workout.date, workout.hasExplicitTime)}</Text>
          <Text style={styles.label}>Duration</Text>
          <Text style={styles.value}>{formatDuration(workout.duration)}</Text>
          <Text style={styles.label}>Notes</Text>
          <Text style={styles.body}>{workout.notes ?? 'No notes recorded for this workout.'}</Text>
        </GlowCard>

        <GlowCard style={styles.secondaryColumn}>
          <Text style={styles.panelTitle}>Actions</Text>
          <View style={styles.actions}>
            <PrimaryButton
              label="Edit workout"
              onPress={() =>
                router.push({
                  pathname: '/(app)/workouts/[id]/edit',
                  params: { id: workout.id },
                } as Href)
              }
              tone="muted"
              style={styles.actionButton}
            />
            <PrimaryButton
              label="Duplicate workout"
              onPress={() =>
                router.push({
                  pathname: '/(app)/workouts/new',
                  params: { duplicateWorkoutId: workout.id },
                } as Href)
              }
              style={styles.actionButton}
            />
            <PrimaryButton
              label="Delete workout"
              onPress={() => deleteMutation.mutate()}
              tone="danger"
              busy={deleteMutation.isPending}
              style={styles.actionButton}
            />
          </View>

          <TextField
            label="Complete in minutes"
            value={completionMinutes}
            onChangeText={setCompletionMinutes}
            keyboardType="number-pad"
            helperText="The API expects a TimeSpan, so the frontend converts minutes to HH:MM:SS."
          />

          <PrimaryButton
            label={workout.isCompleted ? 'Completed' : 'Mark complete'}
            onPress={() => completeMutation.mutate()}
            busy={completeMutation.isPending}
            disabled={workout.isCompleted}
          />

          {actionError ? <Text style={styles.error}>{actionError}</Text> : null}
        </GlowCard>
      </View>

      <GlowCard>
        <Text style={styles.panelTitle}>Linked exercises</Text>
        {workout.exercises.length === 0 ? (
          <Text style={styles.body}>No exercises linked yet.</Text>
        ) : (
          <FlatList
            key={`workout-exercises-${linkedColumns}`}
            data={workout.exercises}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.linkedList}
            columnWrapperStyle={linkedColumns > 1 ? styles.columnWrapper : undefined}
            numColumns={linkedColumns}
            renderItem={({ item }) => (
              <View style={linkedColumns > 1 ? styles.gridColumn : undefined}>
                <ExerciseRow
                  exercise={item}
                  disabled={removeExerciseMutation.isPending || workout.isCompleted}
                  onRemove={() => removeExerciseMutation.mutate(item.id)}
                />
              </View>
            )}
            scrollEnabled={false}
          />
        )}
        <ExerciseSearchPicker
          actionLabel="Link exercise"
          disabled={addExerciseMutation.isPending || workout.isCompleted}
          excludedExerciseIds={workout.exercises.map((exercise) => exercise.id)}
          onAdd={(exercise) => addExerciseMutation.mutate(exercise)}
        />
      </GlowCard>
    </AppScreen>
  );
}

function ExerciseRow({
  exercise,
  disabled,
  onRemove,
}: {
  exercise: WorkoutExercise;
  disabled: boolean;
  onRemove: () => void;
}) {
  return (
    <GlowCard style={styles.linkedCard}>
      <Text style={styles.linkedTitle}>{exercise.name}</Text>
      <Text style={styles.linkedMeta}>
        {exercise.bodyPart} | {exercise.targetMuscle}
      </Text>
      <Text style={styles.body}>{exercise.equipment ?? 'Bodyweight / unspecified'}</Text>
      <PrimaryButton
        label="Remove"
        onPress={onRemove}
        tone="danger"
        disabled={disabled}
      />
    </GlowCard>
  );
}

async function invalidateWorkoutQueries(
  queryClient: QueryClient,
  userId: string | undefined,
  workoutId?: string
) {
  for (const pageSize of [10, 100]) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.workouts.list(userId, 1, pageSize),
    });
    await queryClient.invalidateQueries({
      queryKey: ['workouts', 'list', userId],
    });
  }

  if (workoutId) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.workouts.detail(userId, workoutId),
    });
  }
}

const styles = StyleSheet.create({
  detailColumns: {
    gap: tokens.spacing.lg,
  },
  detailColumnsWide: {
    flexDirection: 'row',
    alignItems: 'flex-start',
  },
  primaryColumn: {
    flex: 1.25,
  },
  secondaryColumn: {
    flex: 1,
  },
  label: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  value: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  body: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 24,
  },
  panelTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  linkedList: {
    gap: tokens.spacing.sm,
  },
  columnWrapper: {
    gap: tokens.spacing.sm,
  },
  gridColumn: {
    flex: 1,
  },
  linkedCard: {
    padding: tokens.spacing.md,
  },
  linkedTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 16,
  },
  linkedMeta: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionButton: {
    minWidth: 150,
    flexGrow: 1,
  },
  error: {
    color: tokens.colors.danger,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
});
