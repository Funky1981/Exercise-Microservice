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
        restDurationSeconds: DEFAULT_REST_SECONDS,
        startedAt: new Date().toISOString(),
        defaultRestSeconds: DEFAULT_REST_SECONDS,
        currentReps: DEFAULT_REPS,
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
      const currentExercise = session.exercises[session.currentExerciseIndex];
      apiClient
        .logSet(session.logId, {
          exerciseId: currentExercise.exerciseId,
          reps: entry.reps,
          durationSeconds: entry.durationSeconds,
          restSeconds: 0,
        })
        .catch(() => console.warn('Failed to log set to backend'));
    }

    // Switch to rest timer
    timer.start();

    updateSession((prev) => {
      const updatedProgress = [...prev.exerciseProgress];
      updatedProgress[prev.currentExerciseIndex] = {
        ...updatedProgress[prev.currentExerciseIndex],
        sets: [...updatedProgress[prev.currentExerciseIndex].sets, entry],
      };

      return {
        ...prev,
        exerciseProgress: updatedProgress,
        timerMode: 'rest' as TimerMode,
        timerState: 'running',
        timerStartedAt: Date.now(),
        timerElapsedBeforePause: 0,
        currentReps: DEFAULT_REPS,
      };
    });
  }, [session, timer, updateSession]);

  const skipRest = useCallback(() => {
    const restDuration = timer.elapsedSeconds;
    timer.reset();
    timer.start();

    updateSession((prev) => {
      // Record rest time on the last completed set
      const updatedProgress = [...prev.exerciseProgress];
      const currentSets = [...updatedProgress[prev.currentExerciseIndex].sets];
      if (currentSets.length > 0) {
        const lastSet = currentSets[currentSets.length - 1];
        currentSets[currentSets.length - 1] = { ...lastSet, restSeconds: restDuration };
        updatedProgress[prev.currentExerciseIndex] = {
          ...updatedProgress[prev.currentExerciseIndex],
          sets: currentSets,
        };
      }

      return {
        ...prev,
        exerciseProgress: updatedProgress,
        timerMode: 'exercise' as TimerMode,
        timerState: 'running',
        timerStartedAt: Date.now(),
        timerElapsedBeforePause: 0,
      };
    });
  }, [timer, updateSession]);

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
      currentReps: DEFAULT_REPS,
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
      currentReps: DEFAULT_REPS,
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
