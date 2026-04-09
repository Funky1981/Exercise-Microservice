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
import type { WorkoutPlanWorkout } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { WorkoutSearchPicker } from '@/features/workouts/workout-search-picker';
import { formatDate, formatDuration } from '@/lib/format';
import { pickResponsiveValue, useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutPlanDetailScreenProps = {
  planId?: string;
};

export function WorkoutPlanDetailScreen({ planId }: WorkoutPlanDetailScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
  const { showToast } = useToast();
  const { breakpoint, isCompact } = useBreakpoint();
  const linkedColumns = pickResponsiveValue(breakpoint, {
    compact: 1,
    medium: 2,
    expanded: 2,
  });
  const planQuery = useQuery({
    queryKey:
      planId && session?.userId
        ? queryKeys.workoutPlans.detail(session.userId, planId)
        : ['workout-plans', 'detail', 'missing'],
    queryFn: () => apiClient.getWorkoutPlanById(planId!),
    enabled: Boolean(planId) && Boolean(session?.userId),
  });

  const activateMutation = useMutation({
    mutationFn: () => apiClient.activateWorkoutPlan(planId!),
    onSuccess: async () => {
      await invalidatePlanQueries(queryClient, session?.userId, planId);
      showToast({
        tone: 'success',
        title: 'Plan activated',
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => apiClient.deleteWorkoutPlan(planId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['workout-plans', 'list', session?.userId],
      });
      showToast({
        tone: 'success',
        title: 'Plan deleted',
      });
      router.replace('/(app)/plans');
    },
  });

  const addWorkoutMutation = useMutation({
    mutationFn: (workout: WorkoutPlanWorkout | { id: string }) =>
      apiClient.addWorkoutToPlan(planId!, { workoutId: workout.id }),
    onSuccess: async () => {
      await invalidatePlanQueries(queryClient, session?.userId, planId);
      showToast({
        tone: 'success',
        title: 'Workout added to plan',
      });
    },
  });

  const removeWorkoutMutation = useMutation({
    mutationFn: (workoutId: string) => apiClient.removeWorkoutFromPlan(planId!, workoutId),
    onSuccess: async () => {
      await invalidatePlanQueries(queryClient, session?.userId, planId);
      showToast({
        tone: 'success',
        title: 'Workout removed from plan',
      });
    },
  });

  if (!planId) {
    return (
      <AppScreen>
        <StatusCard title="Plan missing" body="This route was opened without a plan id." />
      </AppScreen>
    );
  }

  if (planQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading plan" body="Pulling the latest plan detail." busy />
      </AppScreen>
    );
  }

  if (planQuery.isError || !planQuery.data) {
    return (
      <AppScreen>
        <StatusCard
          title="Unable to load plan"
          body={planQuery.error instanceof Error ? planQuery.error.message : 'Try again in a moment.'}
        />
      </AppScreen>
    );
  }

  const plan = planQuery.data;

  return (
    <AppScreen>
      <SectionHeading
        eyebrow={plan.isActive ? 'Active plan' : 'Draft plan'}
        title={plan.name ?? 'Untitled plan'}
        subtitle="Plans are schedules of workout sessions. Review the sessions in this block and adjust them here."
      />

      <View style={[styles.detailColumns, !isCompact && styles.detailColumnsWide]}>
        <GlowCard style={styles.primaryColumn}>
          <Text style={styles.label}>Date window</Text>
          <Text style={styles.value}>
            {formatDate(plan.startDate)} to {plan.endDate ? formatDate(plan.endDate) : 'Open ended'}
          </Text>
          <Text style={styles.label}>Notes</Text>
          <Text style={styles.body}>{plan.notes ?? 'No notes recorded for this plan.'}</Text>
        </GlowCard>

        <GlowCard style={styles.secondaryColumn}>
          <Text style={styles.panelTitle}>Actions</Text>
          <View style={styles.actions}>
            <PrimaryButton
              label="Edit plan"
              onPress={() =>
                router.push({
                  pathname: '/(app)/plans/[id]/edit',
                  params: { id: plan.id },
                } as Href)
              }
              tone="muted"
              style={styles.actionButton}
            />
            <PrimaryButton
              label={plan.isActive ? 'Active now' : 'Activate plan'}
              onPress={() => activateMutation.mutate()}
              busy={activateMutation.isPending}
              disabled={plan.isActive}
              style={styles.actionButton}
            />
            <PrimaryButton
              label="Delete plan"
              onPress={() => deleteMutation.mutate()}
              tone="danger"
              busy={deleteMutation.isPending}
              style={styles.actionButton}
            />
          </View>
          {activateMutation.isError ? (
            <Text style={styles.error}>
              {activateMutation.error instanceof Error
                ? activateMutation.error.message
                : 'Unable to activate plan.'}
            </Text>
          ) : null}
          {deleteMutation.isError ? (
            <Text style={styles.error}>
              {deleteMutation.error instanceof Error
                ? deleteMutation.error.message
                : 'Unable to delete plan.'}
            </Text>
          ) : null}
          {addWorkoutMutation.isError ? (
            <Text style={styles.error}>
              {addWorkoutMutation.error instanceof Error
                ? addWorkoutMutation.error.message
                : 'Unable to add workout to plan.'}
            </Text>
          ) : null}
          {removeWorkoutMutation.isError ? (
            <Text style={styles.error}>
              {removeWorkoutMutation.error instanceof Error
                ? removeWorkoutMutation.error.message
                : 'Unable to remove workout from plan.'}
            </Text>
          ) : null}
        </GlowCard>
      </View>

      <GlowCard>
        <Text style={styles.panelTitle}>Planned workouts</Text>
        {plan.workouts.length === 0 ? (
          <Text style={styles.body}>No workouts linked yet.</Text>
        ) : (
          <FlatList
            key={`plan-workouts-${linkedColumns}`}
            data={plan.workouts}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.linkedList}
            columnWrapperStyle={linkedColumns > 1 ? styles.columnWrapper : undefined}
            numColumns={linkedColumns}
            renderItem={({ item }) => (
              <View style={linkedColumns > 1 ? styles.gridColumn : undefined}>
                <WorkoutRow
                  workout={item}
                  disabled={removeWorkoutMutation.isPending}
                  onOpen={() =>
                    router.push({
                      pathname: '/(app)/workouts/[id]',
                      params: { id: item.id },
                    } as Href)
                  }
                  onRemove={() => removeWorkoutMutation.mutate(item.id)}
                />
              </View>
            )}
            scrollEnabled={false}
          />
        )}
        <WorkoutSearchPicker
          label="Add workout"
          buttonLabel="Link workout"
          excludedWorkoutIds={plan.workouts.map((workout) => workout.id)}
          disabled={addWorkoutMutation.isPending}
          onAdd={(workout) => addWorkoutMutation.mutate(workout)}
        />
      </GlowCard>
    </AppScreen>
  );
}

function WorkoutRow({
  workout,
  disabled,
  onOpen,
  onRemove,
}: {
  workout: WorkoutPlanWorkout;
  disabled: boolean;
  onOpen: () => void;
  onRemove: () => void;
}) {
  return (
    <GlowCard style={styles.linkedCard}>
      <Text style={styles.linkedTitle}>{workout.name ?? 'Untitled workout'}</Text>
      <Text style={styles.linkedMeta}>
        {formatDate(workout.date)} | {workout.isCompleted ? 'Completed' : 'Scheduled'} |{' '}
        {formatDuration(workout.duration)}
      </Text>
      <Text style={styles.body}>{workout.exerciseIds.length} linked exercises</Text>
      <View style={styles.actions}>
        <PrimaryButton label="Open" onPress={onOpen} tone="muted" style={styles.actionButton} />
        <PrimaryButton
          label="Remove"
          onPress={onRemove}
          tone="danger"
          disabled={disabled}
          style={styles.actionButton}
        />
      </View>
    </GlowCard>
  );
}

async function invalidatePlanQueries(
  queryClient: QueryClient,
  userId: string | undefined,
  planId?: string
) {
  for (const pageSize of [8, 100]) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.workoutPlans.list(userId, 1, pageSize),
    });
  }

  await queryClient.invalidateQueries({
    queryKey: ['workout-plans', 'list', userId],
  });

  if (planId) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.workoutPlans.detail(userId, planId),
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
    flex: 1.2,
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
    color: tokens.colors.accentWarm,
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
