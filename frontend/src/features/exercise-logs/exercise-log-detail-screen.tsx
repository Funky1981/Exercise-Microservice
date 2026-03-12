import { useState } from 'react';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import {
  QueryClient,
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { ExerciseLogEntry } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { ExerciseSearchPicker } from '@/features/exercises/exercise-search-picker';
import { formatDateTime, formatDuration, minutesToDuration } from '@/lib/format';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type ExerciseLogDetailScreenProps = {
  logId?: string;
};

export function ExerciseLogDetailScreen({ logId }: ExerciseLogDetailScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const [sets, setSets] = useState('3');
  const [reps, setReps] = useState('10');
  const [minutes, setMinutes] = useState('');
  const [completionMinutes, setCompletionMinutes] = useState('45');

  const logQuery = useQuery({
    queryKey:
      logId && session?.userId
        ? queryKeys.exerciseLogs.detail(session.userId, logId)
        : ['exercise-logs', 'detail', 'missing'],
    queryFn: () => apiClient.getExerciseLogById(logId!),
    enabled: Boolean(logId) && Boolean(session?.userId),
  });

  const exercisesQuery = useQuery({
    queryKey: queryKeys.exercises.catalogue(1, 100),
    queryFn: () => apiClient.getExercises(1, 100),
  });

  const addEntryMutation = useMutation({
    mutationFn: async (exerciseId: string) => {
      const parsedSets = Number(sets);
      const parsedReps = Number(reps);
      const parsedMinutes = minutes ? Number(minutes) : 0;

      if (Number.isNaN(parsedSets) || parsedSets <= 0) {
        throw new Error('Sets must be a positive number.');
      }

      if (Number.isNaN(parsedReps) || parsedReps <= 0) {
        throw new Error('Reps must be a positive number.');
      }

      if (minutes && (Number.isNaN(parsedMinutes) || parsedMinutes < 0)) {
        throw new Error('Duration minutes must be zero or greater.');
      }

      return apiClient.addExerciseLogEntry(logId!, {
        exerciseId,
        sets: parsedSets,
        reps: parsedReps,
        duration: minutes ? minutesToDuration(parsedMinutes) : null,
      });
    },
    onSuccess: async () => {
      setMinutes('');
      await invalidateLogQueries(queryClient, session?.userId, logId);
      showToast({
        tone: 'success',
        title: 'Entry added',
      });
    },
  });

  const completeMutation = useMutation({
    mutationFn: async () => {
      const parsedMinutes = completionMinutes ? Number(completionMinutes) : 0;

      if (completionMinutes && (Number.isNaN(parsedMinutes) || parsedMinutes <= 0)) {
        throw new Error('Completion minutes must be a positive number.');
      }

      return apiClient.completeExerciseLog(
        logId!,
        completionMinutes ? minutesToDuration(parsedMinutes) : null
      );
    },
    onSuccess: async () => {
      await invalidateLogQueries(queryClient, session?.userId, logId);
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
      });
      showToast({
        tone: 'success',
        title: 'Log completed',
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => apiClient.deleteExerciseLog(logId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['exercise-logs', 'list', session?.userId],
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
      });
      showToast({
        tone: 'success',
        title: 'Log deleted',
      });
      router.back();
    },
  });

  if (!logId) {
    return (
      <AppScreen>
        <StatusCard title="Log missing" body="This route was opened without a log id." />
      </AppScreen>
    );
  }

  if (logQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading log" body="Pulling the latest log detail." busy />
      </AppScreen>
    );
  }

  if (logQuery.isError || !logQuery.data) {
    return (
      <AppScreen>
        <StatusCard
          title="Unable to load log"
          body={logQuery.error instanceof Error ? logQuery.error.message : 'Try again in a moment.'}
        />
      </AppScreen>
    );
  }

  const log = logQuery.data;

  return (
    <AppScreen>
      <SectionHeading
        eyebrow={log.isCompleted ? 'Completed log' : 'Live log'}
        title={log.name ?? 'Untitled log'}
        subtitle="This detail screen keeps entry creation, completion, and deletion in one place, with toast feedback for each mutation."
      />

      <GlowCard>
        <Text style={styles.label}>Scheduled time</Text>
        <Text style={styles.value}>{formatDateTime(log.date)}</Text>
        <Text style={styles.label}>Duration</Text>
        <Text style={styles.value}>{formatDuration(log.duration)}</Text>
        <Text style={styles.label}>Notes</Text>
        <Text style={styles.body}>{log.notes ?? 'No notes recorded for this log.'}</Text>
      </GlowCard>

      <GlowCard>
        <Text style={styles.panelTitle}>Add entry</Text>
        <View style={styles.inlineFields}>
          <TextField label="Sets" value={sets} onChangeText={setSets} keyboardType="number-pad" />
          <TextField label="Reps" value={reps} onChangeText={setReps} keyboardType="number-pad" />
          <TextField
            label="Minutes"
            value={minutes}
            onChangeText={setMinutes}
            keyboardType="number-pad"
            helperText="Optional"
          />
        </View>
        <ExerciseSearchPicker
          label="Find exercise"
          buttonLabel="Add entry"
          disabled={addEntryMutation.isPending || log.isCompleted}
          onAdd={(exercise) => addEntryMutation.mutate(exercise.id)}
        />
        {addEntryMutation.isError ? (
          <Text style={styles.error}>
            {addEntryMutation.error instanceof Error
              ? addEntryMutation.error.message
              : 'Unable to add entry.'}
          </Text>
        ) : null}
      </GlowCard>

      <GlowCard>
        <Text style={styles.panelTitle}>Entries</Text>
        {log.entries.length === 0 ? (
          <Text style={styles.body}>No entries added yet.</Text>
        ) : (
          <FlatList
            data={log.entries}
            keyExtractor={(item, index) => `${item.exerciseId}-${index}`}
            renderItem={({ item }) => (
              <ExerciseLogEntryCard
                entry={item}
                exerciseName={
                  exercisesQuery.data?.items.find((exercise) => exercise.id === item.exerciseId)?.name
                }
              />
            )}
            scrollEnabled={false}
            contentContainerStyle={styles.entryList}
          />
        )}
      </GlowCard>

      <GlowCard>
        <Text style={styles.panelTitle}>Close log</Text>
        <TextField
          label="Total minutes"
          value={completionMinutes}
          onChangeText={setCompletionMinutes}
          keyboardType="number-pad"
          helperText="Optional but recommended for analytics."
        />
        <View style={styles.actions}>
          <PrimaryButton
            label={log.isCompleted ? 'Completed' : 'Complete log'}
            onPress={() => completeMutation.mutate()}
            busy={completeMutation.isPending}
            disabled={log.isCompleted}
            style={styles.actionButton}
          />
          <PrimaryButton
            label="Delete log"
            onPress={() => deleteMutation.mutate()}
            busy={deleteMutation.isPending}
            tone="danger"
            style={styles.actionButton}
          />
        </View>
        {completeMutation.isError ? (
          <Text style={styles.error}>
            {completeMutation.error instanceof Error
              ? completeMutation.error.message
              : 'Unable to complete log.'}
          </Text>
        ) : null}
        {deleteMutation.isError ? (
          <Text style={styles.error}>
            {deleteMutation.error instanceof Error
              ? deleteMutation.error.message
              : 'Unable to delete log.'}
          </Text>
        ) : null}
      </GlowCard>
    </AppScreen>
  );
}

function ExerciseLogEntryCard({
  entry,
  exerciseName,
}: {
  entry: ExerciseLogEntry;
  exerciseName?: string;
}) {
  return (
    <GlowCard style={styles.entryCard}>
      <Text style={styles.entryTitle}>{exerciseName ?? entry.exerciseId}</Text>
      <Text style={styles.entryMeta}>
        {entry.sets} sets | {entry.reps} reps | {formatDuration(entry.duration)}
      </Text>
    </GlowCard>
  );
}

async function invalidateLogQueries(
  queryClient: QueryClient,
  userId: string | undefined,
  logId?: string
) {
  for (const pageSize of [8, 20]) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.exerciseLogs.list(userId, 1, pageSize),
    });
  }

  await queryClient.invalidateQueries({
    queryKey: ['exercise-logs', 'list', userId],
  });

  if (logId) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.exerciseLogs.detail(userId, logId),
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
  inlineFields: {
    gap: tokens.spacing.md,
  },
  entryList: {
    gap: tokens.spacing.sm,
  },
  entryCard: {
    padding: tokens.spacing.md,
  },
  entryTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 16,
  },
  entryMeta: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
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
