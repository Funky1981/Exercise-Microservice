import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Image } from 'expo-image';
import * as WebBrowser from 'expo-web-browser';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import {
  getPlayableExerciseMediaUrl,
  getPlayableExercisePreviewUrl,
  hasPlayableExerciseMedia,
  isVideoExerciseMedia,
} from '@/features/exercises/exercise-media';
import { useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

type ExerciseDetailScreenProps = {
  exerciseId?: string;
};

export function ExerciseDetailScreen({ exerciseId }: ExerciseDetailScreenProps) {
  const { isCompact } = useBreakpoint();
  const { session } = useSession();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [selectedWorkoutId, setSelectedWorkoutId] = useState<string | null>(null);
  const [reviewingCandidateId, setReviewingCandidateId] = useState<string | null>(null);
  const isAdmin = session?.role === 'Admin';

  const exerciseQuery = useQuery({
    queryKey: exerciseId ? queryKeys.exercises.detail(exerciseId) : ['exercises', 'detail', 'missing'],
    queryFn: () => apiClient.getExerciseById(exerciseId!),
    enabled: Boolean(exerciseId),
  });

  const workoutsQuery = useQuery({
    queryKey: queryKeys.workouts.list(session?.userId, 1, 100),
    queryFn: () => apiClient.getWorkouts(1, 100),
    enabled: Boolean(session) && Boolean(exerciseId),
  });

  const mediaCandidatesQuery = useQuery({
    queryKey: exerciseId ? queryKeys.exercises.mediaCandidates(exerciseId) : ['exercises', 'media-candidates', 'missing'],
    queryFn: () => apiClient.getExerciseMediaCandidates(exerciseId!),
    enabled: Boolean(exerciseId) && isAdmin,
  });

  const addToWorkoutMutation = useMutation({
    mutationFn: (workoutId: string) =>
      apiClient.addWorkoutExercise(workoutId, { exerciseId: exerciseId! }),
    onSuccess: async () => {
      if (selectedWorkoutId) {
        await queryClient.invalidateQueries({
          queryKey: queryKeys.workouts.detail(session?.userId, selectedWorkoutId),
        });
      }
      showToast({ tone: 'success', title: 'Exercise added to workout' });
      setSelectedWorkoutId(null);
    },
    onError: (err) => {
      showToast({
        tone: 'error',
        title: 'Failed to add',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
  });

  const quickLogMutation = useMutation({
    mutationFn: async () => {
      const exercise = exerciseQuery.data;
      if (!exercise) throw new Error('Exercise not loaded');
      const log = await apiClient.createExerciseLog({
        name: `Quick log: ${exercise.name}`,
        date: new Date().toISOString(),
      });
      await apiClient.addExerciseLogEntry(log.id, {
        exerciseId: exercise.id,
        sets: 1,
        reps: 0,
      });
      return log.id;
    },
    onSuccess: (logId) => {
      showToast({ tone: 'success', title: 'Log created' });
      router.push({
        pathname: '/(app)/logs/[id]',
        params: { id: logId },
      } as Href);
    },
    onError: (err) => {
      showToast({
        tone: 'error',
        title: 'Failed to create log',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
  });

  const approveCandidateMutation = useMutation({
    mutationFn: (candidateId: string) =>
      apiClient.approveExerciseMediaCandidate(exerciseId!, candidateId, 'Approved in the exercise review panel.'),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.detail(exerciseId!) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.mediaCandidates(exerciseId!) }),
      ]);
      setReviewingCandidateId(null);
      showToast({ tone: 'success', title: 'Media candidate approved' });
    },
    onError: (err) => {
      setReviewingCandidateId(null);
      showToast({
        tone: 'error',
        title: 'Approval failed',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
  });

  const rejectCandidateMutation = useMutation({
    mutationFn: (candidateId: string) =>
      apiClient.rejectExerciseMediaCandidate(exerciseId!, candidateId, 'Rejected in the exercise review panel.'),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.detail(exerciseId!) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.mediaCandidates(exerciseId!) }),
      ]);
      setReviewingCandidateId(null);
      showToast({ tone: 'success', title: 'Media candidate rejected' });
    },
    onError: (err) => {
      setReviewingCandidateId(null);
      showToast({
        tone: 'error',
        title: 'Rejection failed',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
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
  const preferredMediaUrl = getPlayableExerciseMediaUrl(exercise);
  const preferredPreviewUrl = getPlayableExercisePreviewUrl(exercise);
  const hasExampleMedia = hasPlayableExerciseMedia(exercise);
  const isVideoDemo = isVideoExerciseMedia(exercise.mediaKind, exercise.mediaUrl);

  // Available workouts that don't already contain this exercise
  const availableWorkouts = (workoutsQuery.data?.items ?? []).filter(
    (w) => !w.isCompleted && !w.exercises.some((e) => e.id === exercise.id)
  );

  return (
    <AppScreen>
      <SectionHeading
        eyebrow={exercise.bodyPart}
        title={exercise.name}
        subtitle={`${exercise.targetMuscle} · ${exercise.equipment ?? 'Bodyweight'}`}
      />

      <GlowCard style={styles.mediaCard}>
        {preferredMediaUrl ? (
          <View style={styles.mediaPlaceholder}>
            {preferredPreviewUrl ? (
              <Image
                contentFit="cover"
                source={{ uri: preferredPreviewUrl }}
                style={styles.mediaPreview}
              />
            ) : null}
            <View style={styles.videoOverlay}>
              <Text style={styles.mediaPlaceholderIcon}>VID</Text>
              <Text style={styles.mediaPlaceholderText}>
                Verified video is available for this exercise.
              </Text>
            </View>
            <PrimaryButton
              label="Open video"
              onPress={() => {
                void WebBrowser.openBrowserAsync(preferredMediaUrl);
              }}
              tone="muted"
            />
          </View>
        ) : (
          <View style={styles.mediaPlaceholder}>
            <Text style={styles.mediaPlaceholderIcon}>FIT</Text>
            <Text style={styles.mediaPlaceholderText}>
              The current provider did not return media for this exercise.
            </Text>
          </View>
        )}
      </GlowCard>

      <GlowCard>
        <Text style={styles.sectionTitle}>Media source</Text>
        <Text style={styles.body}>
          {hasExampleMedia
            ? `Video${exercise.mediaSourceProvider ? ` · ${exercise.mediaSourceProvider}` : ''}`
            : 'No verified example video yet.'}
        </Text>
        {preferredMediaUrl ? (
          <View style={styles.actionsRow}>
            <PrimaryButton
              label="Open source page"
              onPress={() => {
                void WebBrowser.openBrowserAsync(exercise.mediaSourcePageUrl ?? preferredMediaUrl);
              }}
              tone="muted"
              style={styles.actionBtn}
            />
          </View>
        ) : null}
      </GlowCard>

      {isAdmin ? (
        <GlowCard>
          <Text style={styles.sectionTitle}>Media review</Text>
          <Text style={styles.body}>
            Review candidate matches before they become the exercise&apos;s primary media.
          </Text>

          {mediaCandidatesQuery.isPending ? (
            <Text style={styles.body}>Loading candidate matches...</Text>
          ) : mediaCandidatesQuery.isError ? (
            <Text style={styles.body}>
              {mediaCandidatesQuery.error instanceof Error ? mediaCandidatesQuery.error.message : 'Unable to load candidates.'}
            </Text>
          ) : mediaCandidatesQuery.data && mediaCandidatesQuery.data.length > 0 ? (
            <View style={styles.candidateList}>
              {mediaCandidatesQuery.data.map((candidate) => {
                const isBusy = reviewingCandidateId === candidate.id
                  && (approveCandidateMutation.isPending || rejectCandidateMutation.isPending);

                return (
                  <View key={candidate.id} style={styles.candidateCard}>
                    <View style={styles.candidateHeaderRow}>
                      <Text style={styles.candidateProvider}>{candidate.sourceProvider}</Text>
                      <Text style={styles.candidateScore}>{Math.round(candidate.matchScore * 100)}% match</Text>
                    </View>
                    <Text style={styles.candidateTitle}>
                      {candidate.sourceTitle ?? 'Untitled candidate'}
                    </Text>
                    <Text style={styles.body}>
                      Status: {candidate.reviewStatus}{candidate.isSelected ? ' · Active' : ''}
                    </Text>
                    <View style={styles.actionsRow}>
                      <PrimaryButton
                        label="Open source"
                        onPress={() => {
                          const target = candidate.sourcePageUrl ?? candidate.mediaUrl;
                          if (target) {
                            void WebBrowser.openBrowserAsync(target);
                          }
                        }}
                        tone="muted"
                        style={styles.actionBtn}
                      />
                      {candidate.reviewStatus !== 'Approved' && candidate.reviewStatus !== 'AutoAccepted' ? (
                        <PrimaryButton
                          label="Approve"
                          onPress={() => {
                            setReviewingCandidateId(candidate.id);
                            approveCandidateMutation.mutate(candidate.id);
                          }}
                          busy={isBusy && approveCandidateMutation.isPending}
                          style={styles.actionBtn}
                        />
                      ) : null}
                      {candidate.reviewStatus !== 'Rejected' ? (
                        <PrimaryButton
                          label="Reject"
                          onPress={() => {
                            setReviewingCandidateId(candidate.id);
                            rejectCandidateMutation.mutate(candidate.id);
                          }}
                          tone="danger"
                          busy={isBusy && rejectCandidateMutation.isPending}
                          style={styles.actionBtn}
                        />
                      ) : null}
                    </View>
                  </View>
                );
              })}
            </View>
          ) : (
            <Text style={styles.body}>No review candidates have been stored for this exercise yet.</Text>
          )}
        </GlowCard>
      ) : null}

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
        <GlowCard style={styles.metaCard}>
          <Text style={styles.metaLabel}>Category</Text>
          <Text style={styles.metaValue}>{exercise.category ?? 'Not set'}</Text>
        </GlowCard>
        <GlowCard style={styles.metaCard}>
          <Text style={styles.metaLabel}>Catalogue source</Text>
          <Text style={styles.metaValue}>{exercise.sourceProvider ?? 'Unknown'}</Text>
        </GlowCard>
      </View>

      {exercise.secondaryMuscles && exercise.secondaryMuscles.length > 0 ? (
        <GlowCard>
          <Text style={styles.sectionTitle}>Secondary muscles</Text>
          <Text style={styles.body}>{exercise.secondaryMuscles.join(' · ')}</Text>
        </GlowCard>
      ) : null}

      <GlowCard>
        <Text style={styles.sectionTitle}>Description</Text>
        <Text style={styles.body}>
          {exercise.description ?? 'No description is stored for this exercise yet.'}
        </Text>
      </GlowCard>

      {exercise.instructions && exercise.instructions.length > 0 ? (
        <GlowCard>
          <Text style={styles.sectionTitle}>Instructions</Text>
          <View style={styles.instructionsList}>
            {exercise.instructions.map((instruction, index) => (
              <Text key={`${exercise.id}-instruction-${index}`} style={styles.body}>
                {index + 1}. {instruction}
              </Text>
            ))}
          </View>
        </GlowCard>
      ) : null}

      <GlowCard>
        <Text style={styles.sectionTitle}>Actions</Text>
        <View style={styles.actionsRow}>
          <PrimaryButton
            label="Quick log this exercise"
            onPress={() => quickLogMutation.mutate()}
            busy={quickLogMutation.isPending}
            style={styles.actionBtn}
          />
          <PrimaryButton
            label="Create workout with this"
            onPress={() =>
              router.push({
                pathname: '/(app)/workouts/new',
                params: { prefillExerciseId: exercise.id },
              } as Href)
            }
            tone="muted"
            style={styles.actionBtn}
          />
          <PrimaryButton
            label="View trends"
            onPress={() =>
              router.push(`/(app)/analytics/${exercise.id}` as Href)
            }
            tone="muted"
            style={styles.actionBtn}
          />
        </View>
      </GlowCard>

      {availableWorkouts.length > 0 ? (
        <GlowCard>
          <Text style={styles.sectionTitle}>Add to existing workout</Text>
          <View style={styles.workoutList}>
            {availableWorkouts.slice(0, 5).map((w) => (
              <PrimaryButton
                key={w.id}
                label={w.name ?? 'Untitled workout'}
                onPress={() => {
                  setSelectedWorkoutId(w.id);
                  addToWorkoutMutation.mutate(w.id);
                }}
                tone="muted"
                busy={addToWorkoutMutation.isPending && selectedWorkoutId === w.id}
                disabled={addToWorkoutMutation.isPending}
              />
            ))}
          </View>
        </GlowCard>
      ) : null}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  mediaCard: {
    overflow: 'hidden',
    padding: 0,
  },
  mediaPreview: {
    width: '100%',
    minHeight: 260,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  mediaPlaceholder: {
    minHeight: 220,
    alignItems: 'center',
    justifyContent: 'center',
    gap: tokens.spacing.sm,
    backgroundColor: tokens.colors.surfaceStrong,
    padding: tokens.spacing.xl,
  },
  videoOverlay: {
    alignItems: 'center',
    gap: tokens.spacing.sm,
    justifyContent: 'center',
    left: 0,
    padding: tokens.spacing.lg,
    position: 'absolute',
    right: 0,
    top: 0,
  },
  mediaPlaceholderIcon: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.display,
    fontSize: 28,
  },
  mediaPlaceholderText: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
    textAlign: 'center',
  },
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
  actionsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  instructionsList: {
    gap: tokens.spacing.xs,
  },
  actionBtn: {
    flexGrow: 1,
    minWidth: 180,
  },
  workoutList: {
    gap: tokens.spacing.sm,
  },
  candidateList: {
    gap: tokens.spacing.md,
  },
  candidateCard: {
    backgroundColor: tokens.colors.surfaceStrong,
    borderRadius: tokens.radius.lg,
    gap: tokens.spacing.sm,
    padding: tokens.spacing.md,
  },
  candidateHeaderRow: {
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'space-between',
    gap: tokens.spacing.sm,
  },
  candidateProvider: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    letterSpacing: 1,
    textTransform: 'uppercase',
  },
  candidateScore: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
  },
  candidateTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 16,
  },
});
