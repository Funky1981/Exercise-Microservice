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
import { formatDate } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type WorkoutPlanDetailScreenProps = {
  planId?: string;
};

export function WorkoutPlanDetailScreen({ planId }: WorkoutPlanDetailScreenProps) {
  const queryClient = useQueryClient();
  const { session } = useSession();
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
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => apiClient.deleteWorkoutPlan(planId!),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: queryKeys.workoutPlans.list(session?.userId, 1, 20),
      });
      router.back();
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
        subtitle="Activation and deletion both invalidate the list and detail caches so larger devices stay consistent across split views and back navigation."
      />

      <GlowCard>
        <Text style={styles.label}>Date window</Text>
        <Text style={styles.value}>
          {formatDate(plan.startDate)} to {plan.endDate ? formatDate(plan.endDate) : 'Open ended'}
        </Text>
        <Text style={styles.label}>Notes</Text>
        <Text style={styles.body}>{plan.notes ?? 'No notes recorded for this plan.'}</Text>
      </GlowCard>

      <GlowCard>
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
      </GlowCard>
    </AppScreen>
  );
}

async function invalidatePlanQueries(
  queryClient: QueryClient,
  userId: string | undefined,
  planId?: string
) {
  await queryClient.invalidateQueries({
    queryKey: queryKeys.workoutPlans.list(userId, 1, 20),
  });

  if (planId) {
    await queryClient.invalidateQueries({
      queryKey: queryKeys.workoutPlans.detail(userId, planId),
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
