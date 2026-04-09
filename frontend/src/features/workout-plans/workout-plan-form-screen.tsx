import { useEffect, useMemo, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { Exercise, Workout, WorkoutPlanWorkout } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { DateTimeField } from '@/components/ui/date-time-field';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { StatusCard } from '@/components/ui/status-card';
import { TextField } from '@/components/ui/text-field';
import { ExerciseCataloguePicker } from '@/features/exercises/exercise-search-picker';
import { WorkoutSearchPicker } from '@/features/workouts/workout-search-picker';
import {
  formatDate,
  inputDateFromIso,
  isoDateFromInput,
  isoDateTimeFromInput,
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

type ExistingPlanWorkout = {
  kind: 'existing';
  id: string;
  name?: string | null;
  date: string;
  notes?: string | null;
  duration?: string | null;
  isCompleted: boolean;
  exerciseCount: number;
};

type DraftPlanWorkout = {
  kind: 'draft';
  id: string;
  name: string;
  date: string;
  notes: string;
  exercises: Exercise[];
};

type PlannedWorkout = ExistingPlanWorkout | DraftPlanWorkout;

export function WorkoutPlanFormScreen({ mode, planId }: WorkoutPlanFormScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const [name, setName] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [notes, setNotes] = useState('');
  const [plannedWorkouts, setPlannedWorkouts] = useState<PlannedWorkout[]>([]);
  const [draftWorkoutName, setDraftWorkoutName] = useState('');
  const [draftWorkoutDate, setDraftWorkoutDate] = useState('');
  const [draftWorkoutNotes, setDraftWorkoutNotes] = useState('');
  const [draftExercises, setDraftExercises] = useState<Exercise[]>([]);
  const [editingDraftId, setEditingDraftId] = useState<string | null>(null);
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
      const nextStartDate = inputDateFromIso(planQuery.data.startDate);
      setName(planQuery.data.name ?? '');
      setStartDate(nextStartDate);
      setEndDate(inputDateFromIso(planQuery.data.endDate) || defaultPlanEndDate(planQuery.data.startDate));
      setNotes(planQuery.data.notes ?? '');
      setPlannedWorkouts(planQuery.data.workouts.map(mapPlanWorkout));
      setDraftWorkoutDate((current) => current || nextStartDate);
      return;
    }

    if (mode === 'create') {
      const todayIso = new Date().toISOString();
      const today = inputDateFromIso(todayIso);
      setStartDate(today);
      setEndDate(defaultPlanEndDate(todayIso));
      setDraftWorkoutDate((current) => current || today);
    }
  }, [mode, planQuery.data]);

  useEffect(() => {
    if (!draftWorkoutDate && startDate) {
      setDraftWorkoutDate(startDate);
    }
  }, [draftWorkoutDate, startDate]);

  const totalExerciseCount = useMemo(
    () =>
      plannedWorkouts.reduce(
        (total, workout) =>
          total + (workout.kind === 'draft' ? workout.exercises.length : workout.exerciseCount),
        0
      ),
    [plannedWorkouts]
  );

  const mutation = useMutation({
    mutationFn: async () => {
      if (!startDate) {
        throw new Error('Choose a start date.');
      }

      if (plannedWorkouts.length === 0) {
        throw new Error('Add at least one workout before saving the plan.');
      }

      const payload = {
        name: normalizeOptionalText(name),
        startDate: isoDateFromInput(startDate),
        endDate: endDate ? isoDateFromInput(endDate) : normalizeOptionalNullableText(endDate),
        notes: normalizeOptionalText(notes),
      };

      const result =
        mode === 'create'
          ? await apiClient.createWorkoutPlan(payload)
          : (await apiClient.updateWorkoutPlan(planId!, payload), { id: planId! });

      const existingLinkedIds = new Set(planQuery.data?.workouts.map((workout) => workout.id) ?? []);
      const retainedExistingIds = plannedWorkouts
        .filter((workout): workout is ExistingPlanWorkout => workout.kind === 'existing')
        .map((workout) => workout.id);

      const createdDraftIds: string[] = [];
      for (const workout of plannedWorkouts) {
        if (workout.kind !== 'draft') {
          continue;
        }

        const created = await apiClient.createWorkout({
          name: normalizeOptionalText(workout.name),
          date: isoDateTimeFromInput(workout.date, null),
          hasExplicitTime: false,
          notes: normalizeOptionalText(workout.notes),
          exerciseIds: workout.exercises.map((exercise) => exercise.id),
        });

        createdDraftIds.push(created.id);
      }

      const desiredWorkoutIds = [...retainedExistingIds, ...createdDraftIds];

      if (mode === 'create') {
        await Promise.all(
          desiredWorkoutIds.map((workoutId) => apiClient.addWorkoutToPlan(result.id, { workoutId }))
        );
      } else {
        const workoutsToAdd = desiredWorkoutIds.filter((id) => !existingLinkedIds.has(id));
        const workoutsToRemove = [...existingLinkedIds].filter((id) => !retainedExistingIds.includes(id));

        await Promise.all(workoutsToAdd.map((workoutId) => apiClient.addWorkoutToPlan(result.id, { workoutId })));
        await Promise.all(workoutsToRemove.map((workoutId) => apiClient.removeWorkoutFromPlan(result.id, workoutId)));
      }

      return { id: result.id };
    },
    onSuccess: async (result) => {
      await queryClient.invalidateQueries({
        queryKey: ['workout-plans', 'list', session?.userId],
      });
      await queryClient.invalidateQueries({
        queryKey: ['workouts', 'list', session?.userId],
      });
      await queryClient.invalidateQueries({
        queryKey: queryKeys.workoutPlans.detail(session?.userId, result.id),
      });

      showToast({
        tone: 'success',
        title: mode === 'create' ? 'Plan created' : 'Plan updated',
      });

      router.replace({
        pathname: '/(app)/plans/[id]',
        params: { id: result.id },
      } as Href);
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

  if (mode === 'edit' && planQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading plan" body="Preparing the current plan values." busy />
      </AppScreen>
    );
  }

  function resetDraftBuilder() {
    setEditingDraftId(null);
    setDraftWorkoutName('');
    setDraftWorkoutDate(startDate || inputDateFromIso(new Date().toISOString()));
    setDraftWorkoutNotes('');
    setDraftExercises([]);
  }

  function saveDraftWorkout() {
    if (!draftWorkoutDate) {
      setError('Choose a date for this workout.');
      return;
    }

    if (draftExercises.length === 0) {
      setError('Choose at least one exercise for this workout.');
      return;
    }

    setError(null);

    const nextDraft: DraftPlanWorkout = {
      kind: 'draft',
      id: editingDraftId ?? `draft-${Date.now()}`,
      name: draftWorkoutName.trim(),
      date: draftWorkoutDate,
      notes: draftWorkoutNotes.trim(),
      exercises: draftExercises,
    };

    setPlannedWorkouts((current) =>
      editingDraftId
        ? current.map((workout) => (workout.kind === 'draft' && workout.id === editingDraftId ? nextDraft : workout))
        : [...current, nextDraft]
    );

    showToast({
      tone: 'success',
      title: editingDraftId ? 'Workout updated in plan' : 'Workout added to plan',
    });

    resetDraftBuilder();
  }

  function editDraftWorkout(workout: DraftPlanWorkout) {
    setEditingDraftId(workout.id);
    setDraftWorkoutName(workout.name);
    setDraftWorkoutDate(workout.date);
    setDraftWorkoutNotes(workout.notes);
    setDraftExercises(workout.exercises);
    setError(null);
  }

  function removePlannedWorkout(workoutId: string) {
    setPlannedWorkouts((current) => current.filter((item) => item.id !== workoutId));
    if (editingDraftId === workoutId) {
      resetDraftBuilder();
    }
  }

  return (
    <AppScreen>
      <GlowCard>
        <Text style={styles.title}>{mode === 'create' ? 'Build your plan' : 'Edit your plan'}</Text>
        <Text style={styles.body}>
          An exercise is one movement. A workout is one session made of exercises. A plan is your schedule of workouts across a date window.
        </Text>
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Build workouts for this plan</Text>
        <Text style={styles.body}>
          Choose the real exercises for each session here. Reusing an existing saved workout is optional, not required.
        </Text>

        {plannedWorkouts.length === 0 ? (
          <StatusCard
            title="No workouts in this plan yet"
            body="Build a workout from real exercises below, or reuse an existing saved workout."
          />
        ) : (
          <View style={styles.selectedList}>
            {plannedWorkouts.map((workout) => (
              <GlowCard key={workout.id} style={styles.selectedCard}>
                <Text style={styles.resultTitle}>{plannedWorkoutName(workout)}</Text>
                <Text style={styles.resultMeta}>
                  {formatDate(plannedWorkoutDate(workout))} |{' '}
                  {workout.kind === 'draft'
                    ? 'Will be created when you save the plan'
                    : workout.isCompleted
                      ? 'Completed'
                      : 'Saved workout'}
                </Text>
                <Text style={styles.resultBody}>
                  {plannedWorkoutExerciseCount(workout)} exercises
                  {plannedWorkoutNotes(workout) ? ` | ${plannedWorkoutNotes(workout)}` : ''}
                </Text>
                {workout.kind === 'draft' ? (
                  <Text style={styles.helperText}>
                    {workout.exercises.slice(0, 4).map((exercise) => exercise.name).join(', ')}
                    {workout.exercises.length > 4 ? '...' : ''}
                  </Text>
                ) : null}
                <View style={styles.actions}>
                  {workout.kind === 'draft' ? (
                    <PrimaryButton
                      label="Edit draft"
                      onPress={() => editDraftWorkout(workout)}
                      tone="muted"
                      style={styles.actionButton}
                    />
                  ) : null}
                  <PrimaryButton
                    label="Remove"
                    onPress={() => removePlannedWorkout(workout.id)}
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
        <Text style={styles.sectionTitle}>
          {editingDraftId ? 'Edit planned workout' : 'Create a workout inside this plan'}
        </Text>
        <Text style={styles.body}>
          This creates a real workout when you save the plan, using the same exercise catalogue and workout endpoint as the main workout flow.
        </Text>
        <TextField
          label="Workout name"
          value={draftWorkoutName}
          onChangeText={setDraftWorkoutName}
          placeholder="Optional. If blank, the app will name it from the exercises."
        />
        <DateTimeField
          label="Workout date"
          mode="date"
          value={draftWorkoutDate}
          onChange={setDraftWorkoutDate}
          helperText="Pick the day this session belongs in the plan."
        />
        <TextField
          label="Workout notes"
          value={draftWorkoutNotes}
          onChangeText={setDraftWorkoutNotes}
          placeholder="Optional focus or coaching notes for this session."
          multiline
        />

        <ExerciseCataloguePicker
          title="Choose exercises for this workout"
          actionLabel="Select"
          disabled={mutation.isPending}
          selectedExerciseIds={draftExercises.map((exercise) => exercise.id)}
          selectionMode="multi"
          onToggle={(exercise) =>
            setDraftExercises((current) =>
              current.some((item) => item.id === exercise.id)
                ? current.filter((item) => item.id !== exercise.id)
                : [...current, exercise]
            )
          }
        />

        <Text style={styles.summaryValue}>{draftExercises.length} exercises selected</Text>
        <View style={styles.actions}>
          <PrimaryButton
            label={editingDraftId ? 'Update workout in plan' : 'Add workout to plan'}
            onPress={saveDraftWorkout}
            disabled={!draftWorkoutDate || draftExercises.length === 0}
            style={styles.actionButton}
          />
          {editingDraftId ? (
            <PrimaryButton
              label="Cancel edit"
              onPress={resetDraftBuilder}
              tone="muted"
              style={styles.actionButton}
            />
          ) : null}
        </View>
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Reuse an existing saved workout</Text>
        <Text style={styles.body}>
          Use this when you already have a finished workout with the right exercises and just want it in the plan.
        </Text>
        <WorkoutSearchPicker
          label="Find saved workouts"
          buttonLabel="Add saved workout to plan"
          excludedWorkoutIds={plannedWorkouts.map((workout) => workout.id)}
          disabled={mutation.isPending}
          onAdd={(workout) =>
            setPlannedWorkouts((current) =>
              current.some((item) => item.id === workout.id)
                ? current
                : [...current, mapWorkout(workout)]
            )
          }
        />
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Plan window</Text>
        <View style={styles.dateGrid}>
          <View style={styles.dateColumn}>
            <DateTimeField
              label="Start date"
              mode="date"
              value={startDate}
              onChange={(value) => {
                setStartDate(value);
                if (!draftWorkoutDate) {
                  setDraftWorkoutDate(value);
                }
                if (!endDate || value > endDate) {
                  setEndDate(defaultPlanEndDate(`${value}T09:00:00`));
                }
              }}
              helperText="When should this training block begin?"
            />
          </View>
          <View style={styles.dateColumn}>
            <DateTimeField
              label="End date"
              mode="date"
              value={endDate}
              onChange={setEndDate}
              helperText="Optional, but recommended so the block feels bounded."
            />
          </View>
        </View>
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Plan details</Text>
        <TextField
          label="Plan name"
          value={name}
          onChangeText={setName}
          placeholder="Spring strength block"
        />
        <TextField
          label="Plan notes"
          value={notes}
          onChangeText={setNotes}
          placeholder="What should this block focus on overall?"
          multiline
        />
        {error ? <Text style={styles.error}>{error}</Text> : null}
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Summary</Text>
        <Text style={styles.summaryValue}>{plannedWorkouts.length} workouts in this plan</Text>
        <Text style={styles.body}>{totalExerciseCount} exercises across all planned sessions</Text>
        <Text style={styles.body}>
          {startDate ? formatDate(isoDateFromInput(startDate)) : 'No start date'}
          {endDate ? ` to ${formatDate(isoDateFromInput(endDate))}` : ' | Open ended'}
        </Text>
        <View style={styles.actions}>
          <PrimaryButton
            label={mode === 'create' ? 'Save plan' : 'Save changes'}
            onPress={() => mutation.mutate()}
            busy={mutation.isPending}
            disabled={plannedWorkouts.length === 0}
            style={styles.actionButton}
          />
          <PrimaryButton
            label="Cancel"
            onPress={() =>
              router.replace(
                mode === 'edit' && planId
                  ? ({ pathname: '/(app)/plans/[id]', params: { id: planId } } as Href)
                  : '/(app)/plans'
              )
            }
            tone="muted"
            style={styles.actionButton}
          />
        </View>
      </GlowCard>
    </AppScreen>
  );
}

function mapWorkout(workout: Workout): ExistingPlanWorkout {
  return {
    kind: 'existing',
    id: workout.id,
    name: workout.name,
    date: workout.date,
    notes: workout.notes ?? null,
    duration: workout.duration ?? null,
    isCompleted: workout.isCompleted,
    exerciseCount: workout.exercises.length,
  };
}

function mapPlanWorkout(workout: WorkoutPlanWorkout): ExistingPlanWorkout {
  return {
    kind: 'existing',
    id: workout.id,
    name: workout.name,
    date: workout.date,
    notes: null,
    duration: workout.duration ?? null,
    isCompleted: workout.isCompleted,
    exerciseCount: workout.exerciseIds.length,
  };
}

function plannedWorkoutName(workout: PlannedWorkout) {
  if (workout.kind === 'draft') {
    return workout.name || inferWorkoutName(workout.exercises);
  }

  return workout.name ?? 'Untitled workout';
}

function plannedWorkoutDate(workout: PlannedWorkout) {
  return workout.date;
}

function plannedWorkoutNotes(workout: PlannedWorkout) {
  return workout.notes ?? null;
}

function plannedWorkoutExerciseCount(workout: PlannedWorkout) {
  return workout.kind === 'draft' ? workout.exercises.length : workout.exerciseCount;
}

function inferWorkoutName(exercises: Exercise[]) {
  const bodyParts = [...new Set(exercises.map((exercise) => toTitleCase(exercise.bodyPart)))];
  if (bodyParts.length === 0) {
    return 'Untitled workout';
  }

  return bodyParts.slice(0, 2).join(' + ');
}

function toTitleCase(value: string) {
  return value.replace(/\b\w/g, (character) => character.toUpperCase());
}

function defaultPlanEndDate(startDateIso: string) {
  const date = new Date(startDateIso);
  date.setDate(date.getDate() + 7);
  return inputDateFromIso(date.toISOString());
}

const styles = StyleSheet.create({
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 24,
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
  helperText: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 13,
    lineHeight: 20,
  },
  selectedList: {
    gap: tokens.spacing.sm,
  },
  selectedCard: {
    gap: tokens.spacing.sm,
  },
  resultTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 16,
  },
  resultMeta: {
    color: tokens.colors.accentWarm,
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
  dateGrid: {
    gap: tokens.spacing.md,
  },
  dateColumn: {
    flex: 1,
  },
  summaryValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 22,
  },
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionButton: {
    flexGrow: 1,
    minWidth: 180,
  },
  error: {
    color: tokens.colors.danger,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
});
