import { StyleSheet, Text, View } from 'react-native';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

type ExerciseDetailScreenProps = {
  exerciseId?: string;
};

export function ExerciseDetailScreen({ exerciseId }: ExerciseDetailScreenProps) {
  const { isCompact } = useBreakpoint();
  const exerciseQuery = useQuery({
    queryKey: exerciseId ? queryKeys.exercises.detail(exerciseId) : ['exercises', 'detail', 'missing'],
    queryFn: () => apiClient.getExerciseById(exerciseId!),
    enabled: Boolean(exerciseId),
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

  return (
    <AppScreen>
      <SectionHeading
        eyebrow={exercise.bodyPart}
        title={exercise.name}
        subtitle="Exercise detail comes from the shared catalogue endpoint so list and detail views stay in sync."
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
});
