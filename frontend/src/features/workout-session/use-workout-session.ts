import { useCallback, useEffect, useRef, useState } from 'react';

import { apiClient } from '@/api/client';
import type { Workout } from '@/api/types';

import { sessionStorage } from './session-storage';
import type {
  ExerciseProgress,
  SetEntry,
  TimerMode,
  TrainingMode,
  WorkoutSession,
  WorkoutSessionSummary,
} from './types';
import { useTimer } from './use-timer';

const DEFAULT_REST_SECONDS = 60;
const DEFAULT_REPS = 10;
const DEFAULT_TIMED_DURATION = 30;

export function useWorkoutSession() {
  const [session, setSession] = useState<WorkoutSession | null>(null);
  const [isBooting, setIsBooting] = useState(true);
  const timer = useTimer();
  const persistRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const restAdvanceLockRef = useRef(false);

  // ── Persistence helpers ──
  const persist = useCallback((next: WorkoutSession) => {
    // Debounce writes to avoid thrashing AsyncStorage
    if (persistRef.current) clearTimeout(persistRef.current);
    persistRef.current = setTimeout(() => {
      sessionStorage.save(next);
    }, 300);
  }, []);

  const updateSession = useCallback(
    (updater: (prev: WorkoutSession) => WorkoutSession) => {
      setSession((prev) => {
        if (!prev) return prev;
        const next = updater(prev);
        persist(next);
        return next;
      });
    },
    [persist]
  );

  // ── Boot: recover session from storage ──
  useEffect(() => {
    (async () => {
      const saved = await sessionStorage.load();
      if (saved) {
        setSession(saved);
        if (saved.timerState === 'running' && saved.timerStartedAt !== null) {
          timer.restore(saved.timerStartedAt, saved.timerElapsedBeforePause, true);
        } else if (saved.timerState === 'paused') {
          timer.restore(null, saved.timerElapsedBeforePause, false);
        }
      }
      setIsBooting(false);
    })();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Sync timer state back to session on each tick ──
  useEffect(() => {
    if (!session || session.timerState === 'idle') return;
    // We don't re-persist on every frame — only on meaningful state changes
  }, [session, timer.elapsedSeconds]);

  useEffect(() => {
    if (!session || session.timerMode !== 'rest') {
      restAdvanceLockRef.current = false;
      return;
    }

    if (session.timerState !== 'running') {
      return;
    }

    if (timer.elapsedSeconds < session.restDurationSeconds) {
      restAdvanceLockRef.current = false;
      return;
    }

    if (restAdvanceLockRef.current) {
      return;
    }

    restAdvanceLockRef.current = true;

    const isExerciseComplete =
      session.exerciseProgress[session.currentExerciseIndex].sets.length >=
      session.exercises[session.currentExerciseIndex].sets;
    const isLastExerciseComplete =
      isExerciseComplete && session.currentExerciseIndex === session.exercises.length - 1;

    timer.reset();
    if (!isLastExerciseComplete) {
      timer.start();
    }

    updateSession((prev) => {
      const updatedProgress = [...prev.exerciseProgress];
      const currentExercise = prev.exercises[prev.currentExerciseIndex];
      const currentSets = [...updatedProgress[prev.currentExerciseIndex].sets];

      if (currentSets.length > 0) {
        const lastSet = currentSets[currentSets.length - 1];
        currentSets[currentSets.length - 1] = {
          ...lastSet,
          restSeconds: prev.restDurationSeconds,
        };
        updatedProgress[prev.currentExerciseIndex] = {
          ...updatedProgress[prev.currentExerciseIndex],
          sets: currentSets,
        };
      }

      const nextExerciseIndex = isExerciseComplete
        ? Math.min(prev.currentExerciseIndex + 1, prev.exercises.length - 1)
        : prev.currentExerciseIndex;
      const nextExercise = prev.exercises[nextExerciseIndex];

      if (isLastExerciseComplete) {
        return {
          ...prev,
          exerciseProgress: updatedProgress,
          timerMode: 'exercise',
          timerState: 'idle',
          timerStartedAt: null,
          timerElapsedBeforePause: 0,
        };
      }

      return {
        ...prev,
        exerciseProgress: updatedProgress,
        currentExerciseIndex: nextExerciseIndex,
        timerMode: 'exercise',
        timerState: 'running',
        timerStartedAt: Date.now(),
        timerElapsedBeforePause: 0,
        restDurationSeconds: nextExercise.restSeconds,
        defaultRestSeconds: nextExercise.restSeconds,
        currentReps: nextExercise.reps,
      };
    });
  }, [session, timer, updateSession, timer.elapsedSeconds]);

  // ── Actions ──
  const startSession = useCallback(
    async (workout: Workout) => {
      // Fire-and-forget backend call to create the exercise log
      let logId: string | null = null;
      try {
        const result = await apiClient.startSession(workout.id);
        logId = result.logId;
      } catch {
        // Session still starts locally even if backend call fails
        console.warn('Failed to start backend session — continuing offline');
      }

      const exerciseProgress: ExerciseProgress[] = workout.exercises.map((ex) => ({
        exercise: ex,
        sets: [],
      }));

      const newSession: WorkoutSession = {
        workoutId: workout.id,
        workoutName: workout.name ?? 'Untitled workout',
        logId,
        exercises: workout.exercises,
        currentExerciseIndex: 0,
        exerciseProgress,
        timerMode: 'exercise',
        timerState: 'running',
        timerStartedAt: Date.now(),
        timerElapsedBeforePause: 0,
        restDurationSeconds: workout.exercises[0]?.restSeconds ?? DEFAULT_REST_SECONDS,
        startedAt: new Date().toISOString(),
        defaultRestSeconds: workout.exercises[0]?.restSeconds ?? DEFAULT_REST_SECONDS,
        currentReps: workout.exercises[0]?.reps ?? DEFAULT_REPS,
        trainingMode: 'reps',
        timedDurationSeconds: DEFAULT_TIMED_DURATION,
      };

      setSession(newSession);
      sessionStorage.save(newSession);
      timer.start();
    },
    [timer]
  );

  const pauseTimer = useCallback(() => {
    timer.pause();
    updateSession((prev) => ({
      ...prev,
      timerState: 'paused',
      timerStartedAt: null,
      timerElapsedBeforePause: prev.timerElapsedBeforePause + (Date.now() - (prev.timerStartedAt ?? Date.now())),
    }));
  }, [timer, updateSession]);

  const resumeTimer = useCallback(() => {
    timer.resume();
    updateSession((prev) => ({
      ...prev,
      timerState: 'running',
      timerStartedAt: Date.now(),
    }));
  }, [timer, updateSession]);

  const setCurrentReps = useCallback(
    (reps: number) => {
      updateSession((prev) => ({ ...prev, currentReps: Math.max(0, reps) }));
    },
    [updateSession]
  );

  const setTrainingMode = useCallback(
    (mode: TrainingMode) => {
      updateSession((prev) => ({ ...prev, trainingMode: mode }));
    },
    [updateSession]
  );

  const setTimedDuration = useCallback(
    (seconds: number) => {
      updateSession((prev) => ({ ...prev, timedDurationSeconds: Math.max(5, seconds) }));
    },
    [updateSession]
  );

  const completeSet = useCallback(() => {
    if (!session) return;

    const currentExercise = session.exercises[session.currentExerciseIndex];
    const currentProgress = session.exerciseProgress[session.currentExerciseIndex];
    if (currentProgress.sets.length >= currentExercise.sets) return;

    const exerciseDuration = timer.elapsedSeconds;
    timer.reset();

    const isTimed = session.trainingMode === 'timed';

    const entry: SetEntry = {
      setNumber: session.exerciseProgress[session.currentExerciseIndex].sets.length + 1,
      reps: isTimed ? 0 : session.currentReps,
      durationSeconds: isTimed ? session.timedDurationSeconds : exerciseDuration,
      restSeconds: 0, // Will be filled after rest
      completedAt: new Date().toISOString(),
    };

    // Persist set to the backend
    if (session.logId) {
      apiClient
        .logSet(session.logId, {
          exerciseId: currentExercise.exerciseId,
          reps: entry.reps,
          durationSeconds: entry.durationSeconds,
          restSeconds: currentExercise.restSeconds,
        })
        .catch(() => console.warn('Failed to log set to backend'));
    }

    const willFinishExercise = currentProgress.sets.length + 1 >= currentExercise.sets;
    const willFinishWorkout =
      willFinishExercise && session.currentExerciseIndex === session.exercises.length - 1;

    if (!willFinishWorkout) {
      timer.start();
    }

    updateSession((prev) => {
      const updatedProgress = [...prev.exerciseProgress];
      updatedProgress[prev.currentExerciseIndex] = {
        ...updatedProgress[prev.currentExerciseIndex],
        sets: [...updatedProgress[prev.currentExerciseIndex].sets, entry],
      };

      return {
        ...prev,
        exerciseProgress: updatedProgress,
        timerMode: willFinishWorkout ? ('exercise' as TimerMode) : ('rest' as TimerMode),
        timerState: willFinishWorkout ? 'idle' : 'running',
        timerStartedAt: willFinishWorkout ? null : Date.now(),
        timerElapsedBeforePause: 0,
        restDurationSeconds: currentExercise.restSeconds,
        defaultRestSeconds: currentExercise.restSeconds,
        currentReps: currentExercise.reps,
      };
    });
  }, [session, timer, updateSession]);

  const skipRest = useCallback(() => {
    if (!session) return;

    const restDuration = Math.min(timer.elapsedSeconds, session.restDurationSeconds);
    const isExerciseComplete =
      session.exerciseProgress[session.currentExerciseIndex].sets.length >=
      session.exercises[session.currentExerciseIndex].sets;
    const isLastExerciseComplete =
      isExerciseComplete && session.currentExerciseIndex === session.exercises.length - 1;

    timer.reset();
    if (!isLastExerciseComplete) {
      timer.start();
    }

    updateSession((prev) => {
      // Record rest time on the last completed set
      const updatedProgress = [...prev.exerciseProgress];
      const currentExercise = prev.exercises[prev.currentExerciseIndex];
      const currentSets = [...updatedProgress[prev.currentExerciseIndex].sets];
      if (currentSets.length > 0) {
        const lastSet = currentSets[currentSets.length - 1];
        currentSets[currentSets.length - 1] = { ...lastSet, restSeconds: restDuration };
        updatedProgress[prev.currentExerciseIndex] = {
          ...updatedProgress[prev.currentExerciseIndex],
          sets: currentSets,
        };
      }

      const nextExerciseIndex = isExerciseComplete
        ? Math.min(prev.currentExerciseIndex + 1, prev.exercises.length - 1)
        : prev.currentExerciseIndex;
      const nextExercise = prev.exercises[nextExerciseIndex];

      if (isLastExerciseComplete) {
        return {
          ...prev,
          exerciseProgress: updatedProgress,
          timerMode: 'exercise' as TimerMode,
          timerState: 'idle',
          timerStartedAt: null,
          timerElapsedBeforePause: 0,
        };
      }

      return {
        ...prev,
        exerciseProgress: updatedProgress,
        currentExerciseIndex: nextExerciseIndex,
        timerMode: 'exercise' as TimerMode,
        timerState: 'running',
        timerStartedAt: Date.now(),
        timerElapsedBeforePause: 0,
        restDurationSeconds: nextExercise.restSeconds,
        defaultRestSeconds: nextExercise.restSeconds,
        currentReps: nextExercise.reps,
      };
    });
  }, [session, timer, updateSession]);

  const nextExercise = useCallback(() => {
    if (!session) return;
    const nextIndex = session.currentExerciseIndex + 1;
    if (nextIndex >= session.exercises.length) return;

    timer.reset();
    timer.start();

    updateSession((prev) => ({
      ...prev,
      currentExerciseIndex: nextIndex,
      timerMode: 'exercise' as TimerMode,
      timerState: 'running',
      timerStartedAt: Date.now(),
      timerElapsedBeforePause: 0,
      restDurationSeconds: prev.exercises[nextIndex].restSeconds,
      defaultRestSeconds: prev.exercises[nextIndex].restSeconds,
      currentReps: prev.exercises[nextIndex].reps,
    }));
  }, [session, timer, updateSession]);

  const previousExercise = useCallback(() => {
    if (!session || session.currentExerciseIndex === 0) return;
    const prevIndex = session.currentExerciseIndex - 1;

    timer.reset();
    timer.start();

    updateSession((prev) => ({
      ...prev,
      currentExerciseIndex: prevIndex,
      timerMode: 'exercise' as TimerMode,
      timerState: 'running',
      timerStartedAt: Date.now(),
      timerElapsedBeforePause: 0,
      restDurationSeconds: prev.exercises[prevIndex].restSeconds,
      defaultRestSeconds: prev.exercises[prevIndex].restSeconds,
      currentReps: prev.exercises[prevIndex].reps,
    }));
  }, [session, timer, updateSession]);

  const finishSession = useCallback((): WorkoutSessionSummary | null => {
    if (!session) return null;

    timer.reset();

    const totalDurationSeconds = Math.floor(
      (Date.now() - new Date(session.startedAt).getTime()) / 1000
    );

    // End the backend session
    if (session.logId) {
      apiClient
        .endSession(session.logId, totalDurationSeconds)
        .catch(() => console.warn('Failed to end backend session'));
    }

    const summary: WorkoutSessionSummary = {
      workoutId: session.workoutId,
      workoutName: session.workoutName,
      totalDurationSeconds,
      exercises: session.exerciseProgress.map((ep) => ({
        exercise: ep.exercise,
        sets: ep.sets,
        totalSets: ep.sets.length,
        totalReps: ep.sets.reduce((sum, s) => sum + s.reps, 0),
        totalExerciseTime: ep.sets.reduce((sum, s) => sum + s.durationSeconds, 0),
        totalRestTime: ep.sets.reduce((sum, s) => sum + s.restSeconds, 0),
      })),
    };

    setSession(null);
    sessionStorage.clear();

    return summary;
  }, [session, timer]);

  const discardSession = useCallback(() => {
    timer.reset();
    setSession(null);
    sessionStorage.clear();
  }, [timer]);

  return {
    session,
    isBooting,
    timer,
    startSession,
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
  };
}
