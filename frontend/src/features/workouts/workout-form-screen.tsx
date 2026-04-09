import { useEffect, useMemo, useRef, useState } from 'react';
import { router, type Href } from 'expo-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { StyleSheet, Text, View } from 'react-native';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise, Workout } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { DateTimeField } from '@/components/ui/date-time-field';
import { FilterChip } from '@/components/ui/filter-chip';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { ExerciseCataloguePicker } from '@/features/exercises/exercise-search-picker';
import {
  formatWorkoutSchedule,
  inputDateFromIso,
  inputTimeFromIso,
  isoDateTimeFromInput,
  normalizeOptionalText,
} from '@/lib/format';
import { pickResponsiveValue, useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutFormScreenProps = {
  mode: 'create' | 'edit';
  workoutId?: string;
  duplicateWorkoutId?: string;
};

type BuilderExercise = Pick<
  Exercise,
  'id' | 'name' | 'bodyPart' | 'targetMuscle' | 'equipment' | 'gifUrl' | 'difficulty'
>;

export function WorkoutFormScreen({
  mode,
  workoutId,
  duplicateWorkoutId,
}: WorkoutFormScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const { breakpoint, isCompact } = useBreakpoint();
  const [name, setName] = useState('');
  const [date, setDate] = useState('');
  const [time, setTime] = useState('');
  const [hasExplicitTime, setHasExplicitTime] = useState(false);
  const [notes, setNotes] = useState('');
  const [selectedExercises, setSelectedExercises] = useState<BuilderExercise[]>([]);
  const [showNameField, setShowNameField] = useState(false);
  const [showNotesField, setShowNotesField] = useState(false);
  const [showDuplicateList, setShowDuplicateList] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const hydratedTemplateRef = useRef<string | null>(null);

  const recentWorkoutsQuery = useQuery({
    queryKey: queryKeys.workouts.list(session?.userId, 1, 20),
    queryFn: () => apiClient.getWorkouts(1, 20),
    enabled: Boolean(session?.userId),
  });

  const workoutQuery = useQuery({
    queryKey:
      mode === 'edit' && workoutId
        ? queryKeys.workouts.detail(session?.userId, workoutId)
        : ['workouts', 'detail', 'builder-draft'],
    queryFn: () => apiClient.getWorkoutById(workoutId!),
    enabled: mode === 'edit' && Boolean(workoutId) && Boolean(session?.userId),
  });

  const duplicateQuery = useQuery({
    queryKey:
      mode === 'create' && duplicateWorkoutId
        ? ['workouts', 'duplicate-template', session?.userId, duplicateWorkoutId]
        : ['workouts', 'duplicate-template', 'draft'],
    queryFn: () => apiClient.getWorkoutById(duplicateWorkoutId!),
    enabled: mode === 'create' && Boolean(duplicateWorkoutId) && Boolean(session?.userId),
  });

  useEffect(() => {
    if (mode === 'create' && !date) {
      setDate(inputDateFromIso(new Date().toISOString()));
    }
  }, [date, mode]);

  useEffect(() => {
    const source =
      mode === 'edit'
        ? workoutQuery.data
        : duplicateWorkoutId
          ? duplicateQuery.data
          : null;

    if (!source || hydratedTemplateRef.current === source.id) {
      return;
    }

    hydratedTemplateRef.current = source.id;
    hydrateFromWorkout(source);
  }, [duplicateQuery.data, duplicateWorkoutId, mode, workoutQuery.data]);

  function hydrateFromWorkout(workout: Workout) {
    setName(workout.name ?? '');
    setShowNameField(Boolean(workout.name));
    setDate(inputDateFromIso(workout.date));
    setHasExplicitTime(workout.hasExplicitTime);
    setTime(workout.hasExplicitTime ? inputTimeFromIso(workout.date) : '');
    setNotes(workout.notes ?? '');
    setShowNotesField(Boolean(workout.notes));
    setSelectedExercises(
      workout.exercises.map((exercise) => ({
        id: exercise.id,
        name: exercise.name,
        bodyPart: exercise.bodyPart,
        targetMuscle: exercise.targetMuscle,
        equipment: exercise.equipment ?? null,
        gifUrl: null,
        difficulty: null,
      }))
    );
  }

  function resetBuilder() {
    hydratedTemplateRef.current = 'scratch';
    setName('');
    setShowNameField(false);
    setDate(inputDateFromIso(new Date().toISOString()));
    setTime('');
    setHasExplicitTime(false);
    setNotes('');
    setShowNotesField(false);
    setSelectedExercises([]);
    setShowDuplicateList(false);
    setError(null);
  }

  function applyTemplate(workout: Workout) {
    hydratedTemplateRef.current = workout.id;
    hydrateFromWorkout(workout);
    setShowDuplicateList(false);
    setError(null);
  }

  const mutation = useMutation({
    mutationFn: async () => {
      if (!date) {
        throw new Error('Choose a workout date.');
      }

      if (selectedExercises.length === 0) {
        throw new Error('Select at least one exercise before saving.');
      }

      const payload = {
        name: normalizeOptionalText(name),
        date: isoDateTimeFromInput(date, hasExplicitTime ? time : null),
        hasExplicitTime,
        notes: normalizeOptionalText(notes),
        exerciseIds: selectedExercises.map((exercise) => exercise.id),
      };

      if (mode === 'create') {
        return apiClient.createWorkout(payload);
      }

      await apiClient.updateWorkout(workoutId!, payload);
      return { id: workoutId! };
    },
    onSuccess: async (result) => {
      await queryClient.invalidateQueries({
        queryKey: ['workouts', 'list', session?.userId],
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.analytics.summary(session?.userId),
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.workouts.detail(session?.userId, result.id),
      });

      showToast({
        tone: 'success',
        title: mode === 'create' ? 'Workout saved' : 'Workout updated',
      });

      router.replace({
        pathname: '/(app)/workouts/[id]',
        params: { id: result.id },
      } as Href);
    },
    onError: (mutationError) => {
      setError(mutationError instanceof Error ? mutationError.message : 'Unable to save workout.');
    },
  });

  const quickStartWorkouts = recentWorkoutsQuery.data?.items ?? [];
  const lastUpperBodyWorkout = quickStartWorkouts.find((workout) =>
    workout.exercises.length > 0 &&
    workout.exercises.every((exercise) => isUpperBody(exercise.bodyPart))
  );
  const lastLowerBodyWorkout = quickStartWorkouts.find((workout) =>
    workout.exercises.length > 0 &&
    workout.exercises.every((exercise) => isLowerBody(exercise.bodyPart))
  );

  const selectedExerciseIds = selectedExercises.map((exercise) => exercise.id);
  const scheduleLabel = formatWorkoutSchedule(
    date ? isoDateTimeFromInput(date, hasExplicitTime ? time : null) : undefined,
    hasExplicitTime
  );
  const summaryRegions = [...new Set(selectedExercises.map((exercise) => describeRegion(exercise.bodyPart)))];
  const recentColumns = pickResponsiveValue(breakpoint, {
    compact: 1,
    medium: 2,
    expanded: 2,
  });

  if (mode === 'edit' && !workoutId) {
    return (
      <AppScreen>
        <StatusCard title="Workout missing" body="The edit route needs a workout id." />
      </AppScreen>
    );
  }

  if (
    (mode === 'edit' && workoutQuery.isPending) ||
    (mode === 'create' && Boolean(duplicateWorkoutId) && duplicateQuery.isPending)
  ) {
    return (
      <AppScreen>
        <StatusCard title="Loading workout" body="Preparing the builder." busy />
      </AppScreen>
    );
  }

  return (
    <AppScreen>
      <View style={[styles.layout, !isCompact && styles.layoutWide]}>
        <View style={styles.mainColumn}>
          <GlowCard>
            <Text style={styles.title}>
              {mode === 'create' ? 'Build your workout' : 'Edit your workout'}
            </Text>
            <Text style={styles.body}>
              Start with exercises. Add schedule details only when you need them.
            </Text>
          </GlowCard>

          <GlowCard>
            <Text style={styles.sectionTitle}>Quick start</Text>
            <View style={styles.actions}>
              {lastUpperBodyWorkout ? (
                <PrimaryButton
                  label="Repeat last upper body"
                  onPress={() => applyTemplate(lastUpperBodyWorkout)}
                  tone="muted"
                  style={styles.quickStartButton}
                />
              ) : null}
              {lastLowerBodyWorkout ? (
                <PrimaryButton
                  label="Repeat last lower body"
                  onPress={() => applyTemplate(lastLowerBodyWorkout)}
                  tone="muted"
                  style={styles.quickStartButton}
                />
              ) : null}
              <PrimaryButton
                label={showDuplicateList ? 'Hide duplicates' : 'Duplicate existing workout'}
                onPress={() => setShowDuplicateList((current) => !current)}
                tone="muted"
                style={styles.quickStartButton}
              />
              <PrimaryButton
                label="Start from scratch"
                onPress={resetBuilder}
                tone="muted"
                style={styles.quickStartButton}
              />
            </View>
            {showDuplicateList ? (
              <View style={styles.duplicateList}>
                {quickStartWorkouts.length === 0 ? (
                  <Text style={styles.body}>No recent workouts available to duplicate yet.</Text>
                ) : (
                  quickStartWorkouts.slice(0, 6).map((workout, index) => (
                    <GlowCard
                      key={workout.id}
                      style={[
                        styles.duplicateCard,
                        recentColumns > 1 && index % recentColumns === 0 && styles.duplicateCardWide,
                      ]}>
                      <Text style={styles.resultTitle}>{workout.name ?? 'Untitled workout'}</Text>
                      <Text style={styles.resultMeta}>
                        {formatWorkoutSchedule(workout.date, workout.hasExplicitTime)}
                      </Text>
                      <Text style={styles.body}>{workout.notes ?? 'No notes recorded.'}</Text>
                      <PrimaryButton
                        label="Use this workout"
                        onPress={() => applyTemplate(workout)}
                        tone="muted"
                      />
                    </GlowCard>
                  ))
                )}
              </View>
            ) : null}
          </GlowCard>

          <ExerciseCataloguePicker
            actionLabel="Select"
            disabled={mutation.isPending}
            selectedExerciseIds={selectedExerciseIds}
            selectionMode="multi"
            onToggle={(exercise) =>
              setSelectedExercises((current) =>
                current.some((item) => item.id === exercise.id)
                  ? current
                  : [...current, exercise]
              )
            }
          />

          <GlowCard>
            <Text style={styles.sectionTitle}>Selected exercises</Text>
            {selectedExercises.length === 0 ? (
              <Text style={styles.body}>
                Choose at least one exercise to build a valid workout.
              </Text>
            ) : (
              <View style={styles.selectedList}>
                {selectedExercises.map((exercise, index) => (
                  <GlowCard key={exercise.id} style={styles.selectedCard}>
                    <Text style={styles.resultTitle}>{exercise.name}</Text>
                    <Text style={styles.resultMeta}>
                      {exercise.bodyPart} | {exercise.targetMuscle}
                    </Text>
                    <Text style={styles.resultBody}>
                      {exercise.equipment ?? 'Bodyweight / unspecified'}
                    </Text>
                    <View style={styles.actions}>
                      <PrimaryButton
                        label="Move up"
                        onPress={() =>
                          setSelectedExercises((current) => moveExercise(current, index, index - 1))
                        }
                        disabled={index === 0}
                        tone="muted"
                        style={styles.actionButton}
                      />
                      <PrimaryButton
                        label="Move down"
                        onPress={() =>
                          setSelectedExercises((current) => moveExercise(current, index, index + 1))
                        }
                        disabled={index === currentLastIndex(selectedExercises)}
                        tone="muted"
                        style={styles.actionButton}
                      />
                      <PrimaryButton
                        label="Remove"
                        onPress={() => {
                          const removedExercise = exercise;
                          setSelectedExercises((current) =>
                            current.filter((item) => item.id !== exercise.id)
                          );
                          showToast({
                            tone: 'info',
                            title: `${exercise.name} removed`,
                            actionLabel: 'Undo',
                            onAction: () =>
                              setSelectedExercises((current) => {
                                const exists = current.some((item) => item.id === removedExercise.id);
                                if (exists) {
                                  return current;
                                }

                                const next = [...current];
                                next.splice(index, 0, removedExercise);
                                return next;
                              }),
                          });
                        }}
                        tone="danger"
                        style={styles.actionButton}
                      />
                    </View>
                  </GlowCard>
                ))}
              </View>
            )}
          </GlowCard>

          <GlowCard>
            <Text style={styles.sectionTitle}>Schedule</Text>
            <View style={styles.scheduleShortcuts}>
              {[
                { label: 'Today', value: inputDateFromIso(new Date().toISOString()) },
                {
                  label: 'Tomorrow',
                  value: inputDateFromIso(new Date(Date.now() + 86400000).toISOString()),
                },
              ].map((entry) => (
                <FilterChip
                  key={entry.label}
                  label={entry.label}
                  selected={date === entry.value}
                  onPress={() => setDate(entry.value)}
                />
              ))}
              <FilterChip
                label="This evening"
                selected={hasExplicitTime && time === '18:00'}
                onPress={() => {
                  setDate(inputDateFromIso(new Date().toISOString()));
                  setHasExplicitTime(true);
                  setTime('18:00');
                }}
              />
              <FilterChip
                label="Next Monday"
                selected={false}
                onPress={() => {
                  setDate(getNextWeekdayDate(1));
                }}
              />
            </View>
            <View style={styles.filterGrid}>
              <View style={styles.filterColumn}>
                <DateTimeField label="Date" mode="date" value={date} onChange={setDate} />
              </View>
              <View style={styles.filterColumn}>
                <PrimaryButton
                  label={hasExplicitTime ? 'Time set' : 'Any time'}
                  onPress={() => setHasExplicitTime((current) => !current)}
                  tone="muted"
                />
              </View>
            </View>
            {hasExplicitTime ? (
              <DateTimeField
                label="Time"
                mode="time"
                value={time}
                onChange={setTime}
                helperText="Optional, but useful when the session has a real start time."
              />
            ) : (
              <Text style={styles.body}>This workout will show as scheduled for any time that day.</Text>
            )}
          </GlowCard>

          <GlowCard>
            <Text style={styles.sectionTitle}>Details</Text>
            {showNameField ? (
              <TextField
                label="Workout name"
                value={name}
                onChangeText={setName}
                placeholder="Upper body strength"
              />
            ) : (
              <PrimaryButton
                label="Add a name"
                onPress={() => setShowNameField(true)}
                tone="muted"
              />
            )}
            {showNotesField ? (
              <TextField
                label="Notes"
                value={notes}
                onChangeText={setNotes}
                placeholder="What should this session focus on?"
                multiline
              />
            ) : (
              <PrimaryButton
                label="Add notes"
                onPress={() => setShowNotesField(true)}
                tone="muted"
              />
            )}
            {error ? <Text style={styles.error}>{error}</Text> : null}
          </GlowCard>
        </View>

        <View style={[styles.summaryColumn, !isCompact && styles.summaryColumnSticky]}>
          <GlowCard style={styles.summaryCard}>
            <Text style={styles.sectionTitle}>Workout summary</Text>
            <Text style={styles.summaryValue}>{selectedExercises.length} exercises selected</Text>
            <Text style={styles.summaryMeta}>{scheduleLabel}</Text>
            {summaryRegions.length > 0 ? (
              <Text style={styles.body}>Focus: {summaryRegions.join(', ')}</Text>
            ) : null}
            <PrimaryButton
              label={mode === 'create' ? 'Save workout' : 'Save changes'}
              onPress={() => mutation.mutate()}
              busy={mutation.isPending}
              disabled={selectedExercises.length === 0}
            />
            <PrimaryButton
              label="Cancel"
              onPress={() => router.replace('/(app)/(tabs)/workouts')}
              tone="muted"
            />
          </GlowCard>
        </View>
      </View>
    </AppScreen>
  );
}

function moveExercise(list: BuilderExercise[], fromIndex: number, toIndex: number) {
  if (toIndex < 0 || toIndex >= list.length) {
    return list;
  }

  const next = [...list];
  const [exercise] = next.splice(fromIndex, 1);
  next.splice(toIndex, 0, exercise);
  return next;
}

function currentLastIndex(list: BuilderExercise[]) {
  return list.length - 1;
}

function isUpperBody(bodyPart: string) {
  return ['back', 'chest', 'shoulders', 'upper arms', 'lower arms', 'neck'].includes(
    bodyPart.toLowerCase()
  );
}

function isLowerBody(bodyPart: string) {
  return ['upper legs', 'lower legs'].includes(bodyPart.toLowerCase());
}

function describeRegion(bodyPart: string) {
  if (isUpperBody(bodyPart)) {
    return 'Upper body';
  }

  if (isLowerBody(bodyPart)) {
    return 'Lower body';
  }

  if (bodyPart.toLowerCase() === 'waist') {
    return 'Core';
  }

  if (bodyPart.toLowerCase() === 'cardio') {
    return 'Cardio';
  }

  return 'Other';
}

function getNextWeekdayDate(targetDay: number) {
  const date = new Date();
  const difference = (targetDay + 7 - date.getDay()) % 7 || 7;
  date.setDate(date.getDate() + difference);
  return inputDateFromIso(date.toISOString());
}

const styles = StyleSheet.create({
  layout: {
    gap: tokens.spacing.lg,
  },
  layoutWide: {
    flexDirection: 'row',
    alignItems: 'flex-start',
  },
  mainColumn: {
    flex: 1.35,
    gap: tokens.spacing.lg,
  },
  summaryColumn: {
    flex: 0.85,
  },
  summaryColumnSticky: {
    position: 'sticky' as never,
    top: tokens.spacing.lg,
  },
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 26,
  },
  body: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
  sectionTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  quickStartButton: {
    flexGrow: 1,
    minWidth: 180,
  },
  duplicateList: {
    gap: tokens.spacing.md,
  },
  duplicateCard: {
    gap: tokens.spacing.sm,
  },
  duplicateCardWide: {
    flex: 1,
  },
  filterGrid: {
    gap: tokens.spacing.md,
  },
  filterColumn: {
    flex: 1,
  },
  selectedList: {
    gap: tokens.spacing.sm,
  },
  selectedCard: {
    gap: tokens.spacing.sm,
  },
  actionButton: {
    minWidth: 120,
    flexGrow: 1,
  },
  scheduleShortcuts: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  summaryCard: {
    gap: tokens.spacing.md,
  },
  summaryValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 22,
  },
  summaryMeta: {
    color: tokens.colors.accentWarm,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  resultTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 16,
  },
  resultMeta: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  resultBody: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  error: {
    color: tokens.colors.danger,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
});
