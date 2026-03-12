import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { FlatList, StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import type { WorkoutPlan } from '@/api/types';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { formatDate } from '@/lib/format';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

const PAGE_SIZE = 8;

export function WorkoutPlansScreen() {
  const { session } = useSession();
  const [pageNumber, setPageNumber] = useState(1);
  const plansQuery = useQuery({
    queryKey: queryKeys.workoutPlans.list(session?.userId, pageNumber, PAGE_SIZE),
    queryFn: () => apiClient.getWorkoutPlans(pageNumber, PAGE_SIZE),
    enabled: Boolean(session?.userId),
    placeholderData: keepPreviousData,
  });

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Programs"
        title="Workout plans"
        subtitle="Plans live in a secondary stack to keep the main tab bar compact while still supporting full CRUD, activation, and workout association management."
      />

      <PrimaryButton
        label="Create plan"
        onPress={() => router.push('/(app)/plans/new' as Href)}
      />

      {plansQuery.isPending ? (
        <StatusCard title="Loading plans" body="Fetching the latest plan page." busy />
      ) : plansQuery.isError ? (
        <StatusCard
          title="Unable to load plans"
          body={plansQuery.error instanceof Error ? plansQuery.error.message : 'Try again in a moment.'}
        />
      ) : (
        <>
          <FlatList
            data={plansQuery.data?.items ?? []}
            keyExtractor={(item) => item.id}
            contentContainerStyle={styles.list}
            renderItem={({ item }) => <WorkoutPlanCard plan={item} />}
            ListEmptyComponent={
              <StatusCard
                title="No plans yet"
                body="Create a plan to start organizing longer training blocks."
              />
            }
            scrollEnabled={false}
          />
          <PaginationControls
            pageNumber={plansQuery.data?.pageNumber ?? pageNumber}
            totalPages={plansQuery.data?.totalPages ?? 1}
            totalCount={plansQuery.data?.totalCount ?? 0}
            busy={plansQuery.isFetching}
            onPageChange={setPageNumber}
          />
        </>
      )}
    </AppScreen>
  );
}

function WorkoutPlanCard({ plan }: { plan: WorkoutPlan }) {
  return (
    <GlowCard>
      <View style={styles.headerRow}>
        <Text style={styles.name}>{plan.name ?? 'Untitled plan'}</Text>
        <Text style={[styles.badge, plan.isActive && styles.badgeActive]}>
          {plan.isActive ? 'Active' : 'Draft'}
        </Text>
      </View>
      <Text style={styles.meta}>
        {formatDate(plan.startDate)} to {plan.endDate ? formatDate(plan.endDate) : 'Open ended'}
      </Text>
      <Text style={styles.notes}>{plan.notes ?? 'No notes recorded.'}</Text>
      <Text style={styles.metaSecondary}>{plan.workouts.length} linked workouts</Text>
      <PrimaryButton
        label="View plan"
        onPress={() =>
          router.push({
            pathname: '/(app)/plans/[id]',
            params: { id: plan.id },
          } as Href)
        }
        tone="muted"
      />
    </GlowCard>
  );
}

const styles = StyleSheet.create({
  list: {
    gap: tokens.spacing.md,
  },
  headerRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: tokens.spacing.sm,
  },
  name: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 19,
    flex: 1,
  },
  badge: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  badgeActive: {
    color: tokens.colors.success,
  },
  meta: {
    color: tokens.colors.accentWarm,
    fontFamily: tokens.typography.label,
    fontSize: 13,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  metaSecondary: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  notes: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
});
