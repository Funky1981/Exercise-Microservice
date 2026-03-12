import { useDeferredValue, useMemo, useState } from 'react';
import { FlatList, Pressable, StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import {
  QueryClient,
  useMutation,
  useQuery,
  useQueryClient,
} from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise, ExerciseLogEntry } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { formatDateTime, formatDuration, minutesToDuration } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type ExerciseLogDetailScreenProps = {
  logId?: string;
};

export function ExerciseLogDetailScreen({ logId }: ExerciseLogDetailScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const [search, setSearch] = useState('');
  const deferredSearch = useDeferredValue(search);
  const [selectedExercise, setSelectedExercise] = useState<Exercise | null>(null);
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

  const matchingExercises = useMemo(() => {
    const source = exercisesQuery.data?.items ?? [];
    const query = deferredSearch.trim().toLowerCase();

    if (!query) {
      return source.slice(0, 8);
    }

    return source
      .filter((exercise) =>
        [exercise.name, exercise.bodyPart, exercise.targetMuscle]
          .filter(Boolean)
          .some((value) => value.toLowerCase().includes(query))
      )
      .slice(0, 8);
  }, [deferredSearch, exercisesQuery.data?.items]);

  const addEntryMutation = useMutation({
    mutationFn: async () => {
      if (!selectedExercise) {
        throw new Error('Select an exercise before adding an entry.');
      }

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
        exerciseId: selectedExercise.id,
        sets: parsedSets,
        reps: parsedReps,
        duration: minutes ? minutesToDuration(parsedMinutes) : null,
      });
    },
    onSuccess: async () => {
      setSearch('');
      setSelectedExercise(null);
      setMinutes('');
      await invalidateLogQueries(queryClient, session?.userId, logId);
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
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => apiClient.deleteExerciseLog(logId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: queryKeys.exerciseLogs.list(session?.userId, 1, 20),
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
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
        subtitle="This detail screen supports searchable exercise selection, entry creation, completion, and deletion against the current backend contract."
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
        <TextField
          label="Find exercise"
          value={search}
          onChangeText={setSearch}
          placeholder="Bench press, hamstrings, shoulders..."
        />
        <View style={styles.exerciseResults}>
          {matchingExercises.map((exercise) => (
            <Pressable
              key={exercise.id}
              onPress={() => setSelectedExercise(exercise)}
              style={[
                styles.exerciseChip,
                selectedExercise?.id === exercise.id && styles.exerciseChipSelected,
              ]}>
              <Text style={styles.exerciseChipText}>{exercise.name}</Text>
            </Pressable>
          ))}
        </View>
        {selectedExercise ? (
          <Text style={styles.selectionText}>
            Selected: {selectedExercise.name} ({selectedExercise.targetMuscle})
          </Text>
        ) : null}
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
        <PrimaryButton
          label="Add entry"
          onPress={() => addEntryMutation.mutate()}
          busy={addEntryMutation.isPending}
          disabled={log.isCompleted}
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
              <ExerciseLogEntryCard entry={item} exercises={exercisesQuery.data?.items ?? []} />
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
  exercises,
}: {
  entry: ExerciseLogEntry;
  exercises: Exercise[];
}) {
  const exercise = exercises.find((item) => item.id === entry.exerciseId);

  return (
    <GlowCard style={styles.entryCard}>
      <Text style={styles.entryTitle}>{exercise?.name ?? entry.exerciseId}</Text>
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
  await queryClient.invalidateQueries({
    queryKey: queryKeys.exerciseLogs.list(userId, 1, 20),
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
  exerciseResults: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  exerciseChip: {
    borderRadius: tokens.radius.pill,
    borderWidth: 1,
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceRaised,
    paddingHorizontal: tokens.spacing.md,
    paddingVertical: tokens.spacing.sm,
  },
  exerciseChipSelected: {
    borderColor: tokens.colors.accent,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  exerciseChipText: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
  selectionText: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
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
