import { useEffect, useRef, useState } from 'react';
import { Alert, Platform, Pressable, StyleSheet, Text, View, type ViewStyle } from 'react-native';
import { router } from 'expo-router';
import { useQuery } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { StatusCard } from '@/components/ui/status-card';
import { formatRestCountdown, formatTimerDisplay } from '@/lib/format';
import { useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

import type { WorkoutSessionSummary } from './types';
import { useWorkoutSession } from './use-workout-session';

type WorkoutSessionScreenProps = {
  workoutId?: string;
  freshStart?: boolean;
};

export function WorkoutSessionScreen({ workoutId, freshStart = false }: WorkoutSessionScreenProps) {
  const {
    session,
    isBooting,
    timer,
    startSession,
    startExerciseTimer,
    pauseTimer,
    resumeTimer,
    setCurrentReps,
    setTrainingMode,
    setTimedDuration,
    completeSet,
    skipRest,
    nextExercise,
    previousExercise,
    finishSession,
    discardSession,
  } = useWorkoutSession();

  const { session: authSession } = useSession();
  const { showToast } = useToast();
  const { isCompact } = useBreakpoint();
  const [summary, setSummary] = useState<WorkoutSessionSummary | null>(null);
  const [now, setNow] = useState(() => Date.now());
  const hasAutoStarted = useRef(false);
  const hasClearedForFreshStart = useRef(false);
  const shouldIgnoreActiveSession = freshStart && Boolean(workoutId);

  // Fetch the workout if we have a workoutId and no active session yet
  const workoutQuery = useQuery({
    queryKey:
      workoutId && authSession?.userId
        ? queryKeys.workouts.detail(authSession.userId, workoutId)
        : ['workouts', 'detail', 'missing'],
    queryFn: () => apiClient.getWorkoutById(workoutId!),
    enabled:
      Boolean(workoutId) &&
      Boolean(authSession?.userId) &&
      !isBooting &&
      (shouldIgnoreActiveSession || !session),
  });

  useEffect(() => {
    if (!shouldIgnoreActiveSession || isBooting || hasClearedForFreshStart.current) {
      return;
    }

    hasClearedForFreshStart.current = true;
    if (session) {
      discardSession();
    }
    hasAutoStarted.current = false;
  }, [discardSession, isBooting, session, shouldIgnoreActiveSession]);

  useEffect(() => {
    if (!session) {
      return;
    }

    const intervalId = setInterval(() => {
      setNow(Date.now());
    }, 1000);

    return () => clearInterval(intervalId);
  }, [session]);

  // Auto-start session when workout data arrives
  useEffect(() => {
    if (
      workoutQuery.data &&
      !session &&
      !isBooting &&
      !hasAutoStarted.current &&
      workoutQuery.data.exercises.length > 0
    ) {
      hasAutoStarted.current = true;
      startSession(workoutQuery.data);
    }
  }, [workoutQuery.data, session, isBooting, startSession]);

  if (isBooting) {
    return (
      <AppScreen>
        <StatusCard
          title="Loading session"
          body={freshStart ? 'Starting a fresh workout session...' : 'Recovering workout session...'}
          busy
        />
      </AppScreen>
    );
  }

  if (workoutId && workoutQuery.isPending && !session) {
    return (
      <AppScreen>
        <StatusCard
          title="Loading workout"
          body={freshStart ? 'Preparing a brand new workout session...' : 'Preparing your workout session...'}
          busy
        />
      </AppScreen>
    );
  }

  if (summary) {
    return <SessionSummary summary={summary} />;
  }

  if (!session) {
    return (
      <AppScreen>
        <StatusCard
          title="No active session"
          body="Start a workout from the workout detail screen."
        />
        <PrimaryButton
          label="Go to workouts"
          onPress={() => router.replace('/(app)/(tabs)/workouts')}
          tone="muted"
          style={styles.backButton}
        />
      </AppScreen>
    );
  }

  const currentExercise = session.exercises[session.currentExerciseIndex];
  const currentProgress = session.exerciseProgress[session.currentExerciseIndex];
  const isResting = session.timerMode === 'rest';
  const isFirstExercise = session.currentExerciseIndex === 0;
  const isLastExercise = session.currentExerciseIndex === session.exercises.length - 1;
  const isTimed = session.trainingMode === 'timed';
  const hasStartedCurrentSet = !isResting && session.timerState !== 'idle';
  const timedRemaining = Math.max(
    0,
    session.timedDurationSeconds - (hasStartedCurrentSet ? timer.elapsedSeconds : 0)
  );
  const targetSetCount = currentExercise.sets;
  const completedSetCount = currentProgress.sets.length;
  const hasCompletedExercise = completedSetCount >= targetSetCount;
  const isWorkoutReadyToFinish = isLastExercise && hasCompletedExercise && !isResting;
  const nextExercisePreview = !isLastExercise
    ? session.exercises[session.currentExerciseIndex + 1]
    : null;
  const workoutElapsedSeconds = Math.max(
    0,
    Math.floor((now - new Date(session.startedAt).getTime()) / 1000)
  );
  const topStatusBarStyle: ViewStyle | undefined =
    Platform.OS === 'web'
      ? {
          position: 'sticky' as ViewStyle['position'],
          top: 0,
          zIndex: 20,
        }
      : undefined;

  function handleFinish() {
    if (Platform.OS === 'web') {
      if (window.confirm('Finish this workout session?')) {
        const result = finishSession();
        if (result) {
          setSummary(result);
          showToast({ tone: 'success', title: 'Workout complete!' });
        }
      }
    } else {
      Alert.alert('Finish workout', 'Are you sure you want to finish this session?', [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Finish',
          onPress: () => {
            const result = finishSession();
            if (result) {
              setSummary(result);
              showToast({ tone: 'success', title: 'Workout complete!' });
            }
          },
        },
      ]);
    }
  }

  function handleDiscard() {
    if (Platform.OS === 'web') {
      if (window.confirm('Discard this session? All progress will be lost.')) {
        discardSession();
        router.replace('/(app)/(tabs)/workouts');
      }
    } else {
      Alert.alert('Discard session', 'All progress will be lost.', [
        { text: 'Cancel', style: 'cancel' },
        {
          text: 'Discard',
          style: 'destructive',
          onPress: () => {
            discardSession();
            router.replace('/(app)/(tabs)/workouts');
          },
        },
      ]);
    }
  }

  return (
    <AppScreen>
      <GlowCard style={[styles.topStatusBar, topStatusBarStyle]}>
        <View style={styles.topStatusBlock}>
          <Text style={styles.topStatusLabel}>Workout elapsed</Text>
          <Text style={styles.topStatusValue}>{formatTimerDisplay(workoutElapsedSeconds)}</Text>
        </View>
        <View style={styles.topStatusDivider} />
        <View style={styles.topStatusBlock}>
          <Text style={styles.topStatusLabel}>{isResting ? 'Current phase' : 'Current set timer'}</Text>
          <Text style={[styles.topStatusValue, isResting && styles.topStatusValueRest]}>
            {isResting
              ? formatRestCountdown(timer.elapsedSeconds, session.restDurationSeconds)
              : formatTimerDisplay(timer.elapsedSeconds)}
          </Text>
        </View>
      </GlowCard>

      <SectionHeading
        eyebrow={`Exercise ${session.currentExerciseIndex + 1} of ${session.exercises.length}`}
        title={currentExercise.name}
        subtitle={`${currentExercise.bodyPart} · ${currentExercise.targetMuscle} · ${currentExercise.sets} sets · ${currentExercise.reps} reps · ${currentExercise.restSeconds}s rest`}
      />

      {/* ── Timer Display ── */}
      <GlowCard style={styles.timerCard}>
        <Text style={styles.timerLabel}>
          {isResting ? 'REST' : 'EXERCISE'}
        </Text>
        <Text
          style={[
            styles.timerDisplay,
            isResting && styles.timerRest,
            isResting && timer.elapsedSeconds > session.restDurationSeconds && styles.timerOvertime,
          ]}>
          {isResting
            ? formatRestCountdown(timer.elapsedSeconds, session.restDurationSeconds)
            : formatTimerDisplay(timer.elapsedSeconds)}
        </Text>

        <View style={styles.timerActions}>
          {isWorkoutReadyToFinish ? (
            <PrimaryButton label="Workout ready to finish" onPress={handleFinish} style={styles.timerButton} />
          ) : !isResting && session.timerState === 'idle' ? (
            <PrimaryButton
              label={isTimed ? 'Start timed set' : 'Start set timer'}
              onPress={startExerciseTimer}
              style={styles.timerButton}
            />
          ) : session.timerState === 'running' ? (
            <PrimaryButton label="Pause" onPress={pauseTimer} tone="muted" style={styles.timerButton} />
          ) : (
            <PrimaryButton label="Resume" onPress={resumeTimer} style={styles.timerButton} />
          )}
        </View>
      </GlowCard>

      <GlowCard style={styles.nextUpCard}>
        <Text style={styles.nextUpLabel}>Up next</Text>
        <Text style={styles.nextUpTitle}>
          {isResting
            ? `Set ${completedSetCount + 1} of ${targetSetCount}`
            : hasCompletedExercise
              ? nextExercisePreview?.name ?? 'Workout complete'
              : `Set ${completedSetCount + 1} of ${targetSetCount}`}
        </Text>
        <Text style={styles.nextUpBody}>
          {isResting
            ? `Rest ends, then you return to ${currentExercise.name.toLowerCase()} for the next set.`
            : hasCompletedExercise
              ? nextExercisePreview
                ? `Next exercise is ${nextExercisePreview.name.toLowerCase()}. Press Start set timer when you are ready.`
                : 'All exercises are complete. Finish the workout when you are ready.'
              : isTimed
                ? `Timed set target: ${session.timedDurationSeconds}s.`
                : `Current target: ${session.currentReps} reps.`}
        </Text>
      </GlowCard>

      {/* ── Set/Rep Controls (visible during exercise mode) ── */}
      {!isResting ? (
        <GlowCard style={styles.repCard}>
          {hasCompletedExercise ? (
            <Text style={styles.restInfo}>
              {isLastExercise
                ? 'All prescribed sets completed. Finish the workout when you are ready.'
                : 'All prescribed sets completed. Move straight into the next exercise when you are ready.'}
            </Text>
          ) : null}

          {/* Training mode toggle */}
          <View style={styles.modeToggle}>
            <Pressable
              style={[styles.modeButton, !isTimed && styles.modeButtonActive]}
              onPress={() => setTrainingMode('reps')}>
              <Text style={[styles.modeButtonText, !isTimed && styles.modeButtonTextActive]}>
                Reps
              </Text>
            </Pressable>
            <Pressable
              style={[styles.modeButton, isTimed && styles.modeButtonActive]}
              onPress={() => setTrainingMode('timed')}>
              <Text style={[styles.modeButtonText, isTimed && styles.modeButtonTextActive]}>
                Timed
              </Text>
            </Pressable>
          </View>

          {isTimed ? (
            <>
              <Text style={styles.repLabel}>TARGET DURATION</Text>
              <View style={styles.repControls}>
                <Pressable
                  style={styles.repButton}
                  onPress={() => setTimedDuration(session.timedDurationSeconds - 5)}
                  accessibilityLabel="Decrease duration">
                  <Text style={styles.repButtonText}>−</Text>
                </Pressable>
                <Text style={styles.repCount}>{session.timedDurationSeconds}s</Text>
                <Pressable
                  style={styles.repButton}
                  onPress={() => setTimedDuration(session.timedDurationSeconds + 5)}
                  accessibilityLabel="Increase duration">
                  <Text style={styles.repButtonText}>+</Text>
                </Pressable>
              </View>
              <Text style={[styles.timedCountdown, timedRemaining === 0 && styles.timedDone]}>
                {hasStartedCurrentSet
                  ? timedRemaining > 0
                    ? `${timedRemaining}s remaining`
                    : 'Time reached!'
                  : `Press Start timed set to begin the ${session.timedDurationSeconds}s countdown`}
              </Text>
              <PrimaryButton
                label={hasCompletedExercise ? 'Exercise complete' : `Complete Set ${completedSetCount + 1} of ${targetSetCount}`}
                onPress={completeSet}
                disabled={hasCompletedExercise || !hasStartedCurrentSet}
              />
            </>
          ) : (
            <>
              <Text style={styles.repLabel}>REPS</Text>
              <View style={styles.repControls}>
                <Pressable
                  style={styles.repButton}
                  onPress={() => setCurrentReps(session.currentReps - 1)}
                  accessibilityLabel="Decrease reps">
                  <Text style={styles.repButtonText}>−</Text>
                </Pressable>
                <Text style={styles.repCount}>{session.currentReps}</Text>
                <Pressable
                  style={styles.repButton}
                  onPress={() => setCurrentReps(session.currentReps + 1)}
                  accessibilityLabel="Increase reps">
                  <Text style={styles.repButtonText}>+</Text>
                </Pressable>
              </View>
              <Text style={styles.timedCountdown}>
                {hasStartedCurrentSet
                  ? `Set timer: ${formatTimerDisplay(timer.elapsedSeconds)}`
                  : 'Press Start set timer to begin this set timer'}
              </Text>
              <PrimaryButton
                label={hasCompletedExercise ? 'Exercise complete' : `Complete Set ${completedSetCount + 1} of ${targetSetCount}`}
                onPress={completeSet}
                disabled={hasCompletedExercise || !hasStartedCurrentSet}
              />
            </>
          )}
        </GlowCard>
      ) : (
        <GlowCard style={styles.repCard}>
          <Text style={styles.restInfo}>
            Resting after set {completedSetCount} of {targetSetCount} · next {hasCompletedExercise && !isLastExercise ? 'exercise' : 'set'} starts in {formatRestCountdown(timer.elapsedSeconds, session.restDurationSeconds)}
          </Text>
          <PrimaryButton label="Skip rest" onPress={skipRest} tone="muted" />
        </GlowCard>
      )}

      {/* ── Sets Progress ── */}
      {currentProgress.sets.length > 0 ? (
        <GlowCard>
          <Text style={styles.setsTitle}>Sets completed</Text>
          {currentProgress.sets.map((set) => (
            <View key={set.setNumber} style={styles.setRow}>
              <Text style={styles.setText}>Set {set.setNumber}</Text>
              <Text style={styles.setDetail}>
                {set.reps > 0
                  ? `${set.reps} reps · ${formatTimerDisplay(set.durationSeconds)}`
                  : `${formatTimerDisplay(set.durationSeconds)} hold`}
                {set.restSeconds > 0 ? ` · rest ${formatTimerDisplay(set.restSeconds)}` : ''}
              </Text>
            </View>
          ))}
        </GlowCard>
      ) : null}

      {/* ── Navigation ── */}
      <View style={[styles.navRow, isCompact && styles.navRowCompact]}>
        <PrimaryButton
          label="← Prev exercise"
          onPress={previousExercise}
          tone="muted"
          disabled={isFirstExercise}
          style={styles.navButton}
        />
        {!isLastExercise ? (
          <PrimaryButton
            label={hasCompletedExercise ? 'Next exercise →' : 'Skip to next exercise →'}
            onPress={nextExercise}
            tone="muted"
            style={styles.navButton}
          />
        ) : (
          <PrimaryButton
            label="Finish workout"
            onPress={handleFinish}
            style={styles.navButton}
          />
        )}
      </View>

      <PrimaryButton
        label="Discard session"
        onPress={handleDiscard}
        tone="danger"
        style={styles.discardButton}
      />
    </AppScreen>
  );
}

function SessionSummary({ summary }: { summary: WorkoutSessionSummary }) {
  const totalSets = summary.exercises.reduce((s, e) => s + e.totalSets, 0);
  const totalReps = summary.exercises.reduce((s, e) => s + e.totalReps, 0);

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Session complete"
        title={summary.workoutName}
        subtitle={`Total time: ${formatTimerDisplay(summary.totalDurationSeconds)}`}
      />

      <GlowCard>
        <View style={styles.summaryRow}>
          <SummaryStat label="Total Sets" value={String(totalSets)} />
          <SummaryStat label="Total Reps" value={String(totalReps)} />
          <SummaryStat label="Duration" value={formatTimerDisplay(summary.totalDurationSeconds)} />
        </View>
      </GlowCard>

      {summary.exercises.map((ep) => (
        <GlowCard key={ep.exercise.id}>
          <Text style={styles.summaryExerciseName}>{ep.exercise.name}</Text>
          <Text style={styles.summaryMeta}>
            {ep.totalSets} sets · {ep.totalReps} reps · {formatTimerDisplay(ep.totalExerciseTime)} exercise · {formatTimerDisplay(ep.totalRestTime)} rest
          </Text>
          {ep.sets.map((set) => (
            <View key={set.setNumber} style={styles.setRow}>
              <Text style={styles.setText}>Set {set.setNumber}</Text>
              <Text style={styles.setDetail}>
                {set.reps} reps · {formatTimerDisplay(set.durationSeconds)}
              </Text>
            </View>
          ))}
        </GlowCard>
      ))}

      <PrimaryButton
        label="Back to workouts"
        onPress={() => router.replace('/(app)/(tabs)/workouts')}
        style={styles.backButton}
      />
    </AppScreen>
  );
}

function SummaryStat({ label, value }: { label: string; value: string }) {
  return (
    <View style={styles.statItem}>
      <Text style={styles.statLabel}>{label}</Text>
      <Text style={styles.statValue}>{value}</Text>
    </View>
  );
}

const styles = StyleSheet.create({
  topStatusBar: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: tokens.spacing.md,
    paddingVertical: tokens.spacing.sm,
    marginBottom: tokens.spacing.xs,
  },
  topStatusBlock: {
    flex: 1,
    alignItems: 'center',
    gap: 2,
  },
  topStatusLabel: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.label,
    fontSize: 11,
    textTransform: 'uppercase',
    letterSpacing: 1.2,
  },
  topStatusValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  topStatusValueRest: {
    color: tokens.colors.accentWarm,
  },
  topStatusDivider: {
    width: 1,
    alignSelf: 'stretch',
    backgroundColor: tokens.colors.border,
  },
  timerCard: {
    alignItems: 'center',
    paddingVertical: tokens.spacing.xl,
  },
  timerLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 14,
    textTransform: 'uppercase',
    letterSpacing: 2,
  },
  timerDisplay: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.display,
    fontSize: 64,
    lineHeight: 76,
  },
  timerRest: {
    color: tokens.colors.accentWarm,
  },
  timerOvertime: {
    color: tokens.colors.danger,
  },
  timerActions: {
    flexDirection: 'row',
    gap: tokens.spacing.sm,
    marginTop: tokens.spacing.md,
  },
  timerButton: {
    minWidth: 120,
  },
  repCard: {
    alignItems: 'center',
    gap: tokens.spacing.md,
  },
  nextUpCard: {
    gap: tokens.spacing.xs,
  },
  nextUpLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  nextUpTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 22,
  },
  nextUpBody: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
    lineHeight: 22,
  },
  repLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 2,
  },
  repControls: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: tokens.spacing.lg,
  },
  repButton: {
    width: 48,
    height: 48,
    borderRadius: tokens.radius.pill,
    backgroundColor: tokens.colors.surfaceStrong,
    borderWidth: 1,
    borderColor: tokens.colors.border,
    justifyContent: 'center',
    alignItems: 'center',
  },
  repButtonText: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.display,
    fontSize: 24,
    lineHeight: 28,
  },
  repCount: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.display,
    fontSize: 48,
    lineHeight: 56,
    minWidth: 80,
    textAlign: 'center',
  },
  modeToggle: {
    flexDirection: 'row',
    borderRadius: tokens.radius.pill,
    backgroundColor: tokens.colors.surfaceStrong,
    overflow: 'hidden',
    alignSelf: 'center',
  },
  modeButton: {
    paddingVertical: tokens.spacing.xs,
    paddingHorizontal: tokens.spacing.lg,
  },
  modeButtonActive: {
    backgroundColor: tokens.colors.accent,
    borderRadius: tokens.radius.pill,
  },
  modeButtonText: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
  modeButtonTextActive: {
    color: tokens.colors.surface,
  },
  timedCountdown: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.display,
    fontSize: 28,
    textAlign: 'center',
  },
  timedDone: {
    color: tokens.colors.success,
  },
  restInfo: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 16,
  },
  setsTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  setRow: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingVertical: tokens.spacing.xs,
    borderBottomWidth: 1,
    borderBottomColor: tokens.colors.borderSoft,
  },
  setText: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
  setDetail: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  navRow: {
    flexDirection: 'row',
    gap: tokens.spacing.sm,
    marginTop: tokens.spacing.md,
  },
  navRowCompact: {
    flexDirection: 'column',
  },
  navButton: {
    flex: 1,
  },
  discardButton: {
    marginTop: tokens.spacing.sm,
  },
  backButton: {
    marginTop: tokens.spacing.lg,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-around',
  },
  statItem: {
    alignItems: 'center',
    gap: tokens.spacing.xs,
  },
  statLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 11,
    textTransform: 'uppercase',
    letterSpacing: 1,
  },
  statValue: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.display,
    fontSize: 28,
  },
  summaryExerciseName: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  summaryMeta: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
});
