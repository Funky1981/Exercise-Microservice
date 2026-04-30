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
import { formatDateTime } from '@/lib/format';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

const REVIEW_LIMIT = 100;

export function ExerciseMediaReviewScreen() {
  const { session } = useSession();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [reviewingCandidateId, setReviewingCandidateId] = useState<string | null>(null);

  const queueQuery = useQuery({
    queryKey: queryKeys.exercises.mediaReviewQueue(REVIEW_LIMIT),
    queryFn: () => apiClient.getExerciseMediaReviewQueue(REVIEW_LIMIT),
    enabled: session?.role === 'Admin',
  });

  const approveMutation = useMutation({
    mutationFn: ({ exerciseId, candidateId }: { exerciseId: string; candidateId: string }) =>
      apiClient.approveExerciseMediaCandidate(exerciseId, candidateId, 'Approved in the admin review queue.'),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.mediaReviewQueue(REVIEW_LIMIT) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.detail(variables.exerciseId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.mediaCandidates(variables.exerciseId) }),
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

  const rejectMutation = useMutation({
    mutationFn: ({ exerciseId, candidateId }: { exerciseId: string; candidateId: string }) =>
      apiClient.rejectExerciseMediaCandidate(exerciseId, candidateId, 'Rejected in the admin review queue.'),
    onSuccess: async (_, variables) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.mediaReviewQueue(REVIEW_LIMIT) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.detail(variables.exerciseId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.exercises.mediaCandidates(variables.exerciseId) }),
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

  if (session?.role !== 'Admin') {
    return (
      <AppScreen>
        <StatusCard title="Admin only" body="This review queue is only available to admin users." />
      </AppScreen>
    );
  }

  if (queueQuery.isPending) {
    return (
      <AppScreen>
        <StatusCard title="Loading media review queue" body="Fetching pending candidate matches across the exercise catalogue." busy />
      </AppScreen>
    );
  }

  if (queueQuery.isError) {
    return (
      <AppScreen>
        <StatusCard
          title="Unable to load queue"
          body={queueQuery.error instanceof Error ? queueQuery.error.message : 'Try again in a moment.'}
        />
      </AppScreen>
    );
  }

  const queue = queueQuery.data ?? [];

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Admin"
        title="Media review queue"
        subtitle={`Reviewing the top ${REVIEW_LIMIT} pending exercise media matches from the current sync pipeline.`}
      />

      <GlowCard>
        <Text style={styles.summaryText}>{queue.length} pending candidates currently need review.</Text>
      </GlowCard>

      {queue.length === 0 ? (
        <StatusCard title="Queue is empty" body="No pending candidate matches are waiting for review right now." />
      ) : (
        <View style={styles.queueList}>
          {queue.map((item) => {
            const previewUrl = item.mediaKind?.toLowerCase().startsWith('video')
              ? (item.thumbnailUrl ?? null)
              : (item.thumbnailUrl ?? item.mediaUrl);
            const isBusy = reviewingCandidateId === item.id
              && (approveMutation.isPending || rejectMutation.isPending);

            return (
              <GlowCard key={item.id} style={styles.queueCard}>
                {previewUrl ? (
                  <Image contentFit="cover" source={{ uri: previewUrl }} style={styles.preview} />
                ) : (
                  <View style={[styles.preview, styles.previewPlaceholder]}>
                    <Text style={styles.previewPlaceholderText}>No preview</Text>
                  </View>
                )}

                <Text style={styles.exerciseName}>{item.exerciseName}</Text>
                <Text style={styles.metaText}>
                  {item.exerciseBodyPart} · {item.exerciseTargetMuscle}
                  {item.exerciseEquipment ? ` · ${item.exerciseEquipment}` : ''}
                </Text>
                <Text style={styles.metaText}>
                  {Math.round(item.matchScore * 100)}% match · {item.sourceProvider}
                </Text>
                <Text style={styles.bodyText}>{item.sourceTitle ?? 'Untitled media candidate'}</Text>
                <Text style={styles.metaText}>Queued {formatDateTime(item.createdAt)}</Text>

                <View style={styles.actionsRow}>
                  <PrimaryButton
                    label="Exercise detail"
                    onPress={() =>
                      router.push({
                        pathname: '/(app)/exercises/[id]',
                        params: { id: item.exerciseId },
                      } as Href)
                    }
                    tone="muted"
                    style={styles.actionButton}
                  />
                  <PrimaryButton
                    label="Open source"
                    onPress={() => {
                      void WebBrowser.openBrowserAsync(item.sourcePageUrl ?? item.mediaUrl);
                    }}
                    tone="muted"
                    style={styles.actionButton}
                  />
                  <PrimaryButton
                    label="Approve"
                    onPress={() => {
                      setReviewingCandidateId(item.id);
                      approveMutation.mutate({ exerciseId: item.exerciseId, candidateId: item.id });
                    }}
                    busy={isBusy && approveMutation.isPending}
                    style={styles.actionButton}
                  />
                  <PrimaryButton
                    label="Reject"
                    onPress={() => {
                      setReviewingCandidateId(item.id);
                      rejectMutation.mutate({ exerciseId: item.exerciseId, candidateId: item.id });
                    }}
                    tone="danger"
                    busy={isBusy && rejectMutation.isPending}
                    style={styles.actionButton}
                  />
                </View>
              </GlowCard>
            );
          })}
        </View>
      )}
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  summaryText: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
  },
  queueList: {
    gap: tokens.spacing.md,
  },
  queueCard: {
    gap: tokens.spacing.sm,
  },
  preview: {
    width: '100%',
    height: 220,
    borderRadius: tokens.radius.lg,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  previewPlaceholder: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  previewPlaceholderText: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  exerciseName: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  metaText: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
  },
  bodyText: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
  actionsRow: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionButton: {
    flexGrow: 1,
    minWidth: 170,
  },
});