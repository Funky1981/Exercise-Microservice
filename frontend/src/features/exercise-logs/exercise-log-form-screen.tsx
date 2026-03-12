import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router } from 'expo-router';
import { useMutation, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { TextField } from '@/components/ui/text-field';
import { inputDateFromIso, isoDateFromInput, normalizeOptionalText } from '@/lib/format';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export function ExerciseLogFormScreen() {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const [name, setName] = useState('');
  const [date, setDate] = useState(inputDateFromIso(new Date().toISOString()));
  const [notes, setNotes] = useState('');
  const [error, setError] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: async () => {
      if (!date) {
        throw new Error('Enter a date in YYYY-MM-DD format.');
      }

      return apiClient.createExerciseLog({
        name: normalizeOptionalText(name),
        date: isoDateFromInput(date),
        notes: normalizeOptionalText(notes),
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['exercise-logs', 'list', session?.userId],
      });
      showToast({
        tone: 'success',
        title: 'Exercise log created',
      });
      router.back();
    },
    onError: (mutationError) => {
      setError(mutationError instanceof Error ? mutationError.message : 'Unable to create log.');
    },
  });

  return (
    <AppScreen>
      <GlowCard>
        <Text style={styles.title}>Create exercise log</Text>
        <Text style={styles.body}>
          Logs are session containers. Add entries after creation so exercise selection can stay searchable.
        </Text>

        <TextField
          label="Log name"
          value={name}
          onChangeText={setName}
          placeholder="Saturday accessory work"
        />
        <TextField
          label="Date"
          value={date}
          onChangeText={setDate}
          placeholder="YYYY-MM-DD"
          autoCapitalize="none"
        />
        <TextField
          label="Notes"
          value={notes}
          onChangeText={setNotes}
          placeholder="What should this log capture?"
          multiline
        />

        {error ? <Text style={styles.error}>{error}</Text> : null}

        <View style={styles.actions}>
          <PrimaryButton
            label="Create log"
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
