import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import {
  QueryClient,
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { formatDateTime, formatDuration, minutesToDuration } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutDetailScreenProps = {
  workoutId?: string;
};

export function WorkoutDetailScreen({ workoutId }: WorkoutDetailScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const [completionMinutes, setCompletionMinutes] = useState('45');
  const [actionError, setActionError] = useState<string | null>(null);

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
        queryKey: queryKeys.workouts.list(session?.userId, 1, 20),
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
      });
      router.back();
    },
    onError: (mutationError) => {
      setActionError(
        mutationError instanceof Error ? mutationError.message : 'Unable to delete workout.'
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
        subtitle="Detail mutations invalidate both the list and the active detail query so returning to the list never shows stale state."
      />

      <GlowCard>
        <Text style={styles.label}>Scheduled time</Text>
        <Text style={styles.value}>{formatDateTime(workout.date)}</Text>
        <Text style={styles.label}>Duration</Text>
        <Text style={styles.value}>{formatDuration(workout.duration)}</Text>
        <Text style={styles.label}>Notes</Text>
        <Text style={styles.body}>{workout.notes ?? 'No notes recorded for this workout.'}</Text>
      </GlowCard>

      <GlowCard>
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
    </AppScreen>
  );
}

async function invalidateWorkoutQueries(
  queryClient: QueryClient,
  userId: string | undefined,
  workoutId?: string
) {
  await queryClient.invalidateQueries({
    queryKey: queryKeys.workouts.list(userId, 1, 20),
  });

  if (workoutId) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.workouts.detail(userId, workoutId),
    });
  }
}

const styles = StyleSheet.create({
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
