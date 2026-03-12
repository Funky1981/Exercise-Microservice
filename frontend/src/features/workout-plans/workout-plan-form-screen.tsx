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
import {
  inputDateFromIso,
  isoDateFromInput,
  normalizeOptionalNullableText,
  normalizeOptionalText,
} from '@/lib/format';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutPlanFormScreenProps = {
  mode: 'create' | 'edit';
  planId?: string;
};

export function WorkoutPlanFormScreen({ mode, planId }: WorkoutPlanFormScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [notes, setNotes] = useState('');
  const [error, setError] = useState<string | null>(null);

  const planQuery = useQuery({
    queryKey:
      mode === 'edit' && planId
        ? queryKeys.workoutPlans.detail(session?.userId, planId)
        : ['workout-plans', 'detail', 'draft'],
    queryFn: () => apiClient.getWorkoutPlanById(planId!),
    enabled: mode === 'edit' && Boolean(planId) && Boolean(session?.userId),
  });

  useEffect(() => {
    if (planQuery.data) {
      setName(planQuery.data.name ?? '');
      setStartDate(inputDateFromIso(planQuery.data.startDate));
      setEndDate(inputDateFromIso(planQuery.data.endDate));
      setNotes(planQuery.data.notes ?? '');
    } else if (mode === 'create') {
      setStartDate(inputDateFromIso(new Date().toISOString()));
    }
  }, [mode, planQuery.data]);

  const mutation = useMutation({
    mutationFn: async () => {
      if (!startDate) {
        throw new Error('Enter a start date in YYYY-MM-DD format.');
      }

      const payload = {
        name: normalizeOptionalText(name),
        startDate: isoDateFromInput(startDate),
        endDate: endDate ? isoDateFromInput(endDate) : normalizeOptionalNullableText(endDate),
        notes: normalizeOptionalText(notes),
      };

      if (mode === 'create') {
        return apiClient.createWorkoutPlan(payload);
      }

      return apiClient.updateWorkoutPlan(planId!, payload);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['workout-plans', 'list', session?.userId],
      });

      if (planId) {
        await queryClient.invalidateQueries({
          queryKey: queryKeys.workoutPlans.detail(session?.userId, planId),
        });
      }

      showToast({
        tone: 'success',
        title: mode === 'create' ? 'Plan created' : 'Plan updated',
      });
      router.back();
    },
    onError: (mutationError) => {
      setError(mutationError instanceof Error ? mutationError.message : 'Unable to save plan.');
    },
  });

  if (mode === 'edit' && !planId) {
    return (
      <AppScreen>
        <StatusCard title="Plan missing" body="The edit route needs a plan id." />
      </AppScreen>
    );
  }

  if (planQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading plan" body="Preparing the current plan values." busy />
      </AppScreen>
    );
  }

  return (
    <AppScreen>
      <GlowCard>
        <Text style={styles.title}>{mode === 'create' ? 'Create plan' : 'Edit plan'}</Text>
        <Text style={styles.body}>
          Plans keep date ranges explicit so tablet and desktop views can surface training windows clearly.
        </Text>

        <TextField
          label="Plan name"
          value={name}
          onChangeText={setName}
          placeholder="12-week hypertrophy block"
        />
        <TextField
          label="Start date"
          value={startDate}
          onChangeText={setStartDate}
          placeholder="YYYY-MM-DD"
          autoCapitalize="none"
        />
        <TextField
          label="End date"
          value={endDate}
          onChangeText={setEndDate}
          placeholder="YYYY-MM-DD"
          autoCapitalize="none"
          helperText="Optional. Leave blank for an open-ended plan."
        />
        <TextField
          label="Notes"
          value={notes}
          onChangeText={setNotes}
          placeholder="How should this block progress over time?"
          multiline
        />

        {error ? <Text style={styles.error}>{error}</Text> : null}

        <View style={styles.actions}>
          <PrimaryButton
            label={mode === 'create' ? 'Create plan' : 'Save changes'}
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
