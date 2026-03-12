import { useEffect, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { inputDateFromIso, isoDateFromInput, normalizeOptionalText } from '@/lib/format';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutFormScreenProps = {
  mode: 'create' | 'edit';
  workoutId?: string;
};

export function WorkoutFormScreen({ mode, workoutId }: WorkoutFormScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const [name, setName] = useState('');
  const [date, setDate] = useState('');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState<string | null>(null);

  const workoutQuery = useQuery({
    queryKey:
      mode === 'edit' && workoutId
        ? queryKeys.workouts.detail(session?.userId, workoutId)
        : ['workouts', 'detail', 'draft'],
    queryFn: () => apiClient.getWorkoutById(workoutId!),
    enabled: mode === 'edit' && Boolean(workoutId) && Boolean(session?.userId),
  });

  useEffect(() => {
    if (workoutQuery.data) {
      setName(workoutQuery.data.name ?? '');
      setDate(inputDateFromIso(workoutQuery.data.date));
      setNotes(workoutQuery.data.notes ?? '');
    } else if (mode === 'create') {
      setDate(inputDateFromIso(new Date().toISOString()));
    }
  }, [mode, workoutQuery.data]);

  const mutation = useMutation({
    mutationFn: async () => {
      if (!date) {
        throw new Error('Enter a date in YYYY-MM-DD format.');
      }

      const payload = {
        name: normalizeOptionalText(name),
        date: isoDateFromInput(date),
        notes: normalizeOptionalText(notes),
      };

      if (mode === 'create') {
        return apiClient.createWorkout(payload);
      }

      return apiClient.updateWorkout(workoutId!, payload);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['workouts', 'list', session?.userId],
      });

      if (workoutId) {
        await queryClient.invalidateQueries({
          queryKey: queryKeys.workouts.detail(session?.userId, workoutId),
        });
      }

      showToast({
        tone: 'success',
        title: mode === 'create' ? 'Workout created' : 'Workout updated',
      });
      router.back();
    },
    onError: (mutationError) => {
      setError(mutationError instanceof Error ? mutationError.message : 'Unable to save workout.');
    },
  });

  if (mode === 'edit' && !workoutId) {
    return (
      <AppScreen>
        <StatusCard title="Workout missing" body="The edit route needs a workout id." />
      </AppScreen>
    );
  }

  if (workoutQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading workout" body="Preparing the current workout values." busy />
      </AppScreen>
    );
  }

  return (
    <AppScreen>
      <GlowCard>
        <Text style={styles.title}>{mode === 'create' ? 'Create workout' : 'Edit workout'}</Text>
        <Text style={styles.body}>
          Use ISO-friendly dates so the same form works across iOS, Android, and web without a
          platform-specific picker dependency.
        </Text>

        <TextField
          label="Workout name"
          value={name}
          onChangeText={setName}
          placeholder="Upper body strength"
        />
        <TextField
          label="Date"
          value={date}
          onChangeText={setDate}
          placeholder="YYYY-MM-DD"
          autoCapitalize="none"
          helperText="Saved as an ISO timestamp on the backend."
        />
        <TextField
          label="Notes"
          value={notes}
          onChangeText={setNotes}
          placeholder="What should this session focus on?"
          multiline
        />

        {error ? <Text style={styles.error}>{error}</Text> : null}

        <View style={styles.actions}>
          <PrimaryButton
            label={mode === 'create' ? 'Create workout' : 'Save changes'}
            onPress={() => mutation.mutate()}
            busy={mutation.isPending}
            style={styles.actionButton}
          />
          <PrimaryButton
            label="Cancel"
            onPress={() => router.back()}
            tone="muted"
            style={styles.actionButton}
          />
        </View>
      </GlowCard>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 24,
  },
  body: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 24,
  },
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionButton: {
    flexGrow: 1,
    minWidth: 160,
  },
  error: {
    color: tokens.colors.danger,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
});
