import type { WorkoutExercise } from '@/api/types';

export type TimerState = 'idle' | 'running' | 'paused';

export type TimerMode = 'exercise' | 'rest';

/** How the user tracks effort: counting reps or timing a hold/duration */
export type TrainingMode = 'reps' | 'timed';

export type SetEntry = {
  setNumber: number;
  reps: number;
  durationSeconds: number;
  restSeconds: number;
  completedAt: string;
};

export type ExerciseProgress = {
  exercise: WorkoutExercise;
  sets: SetEntry[];
};

export type WorkoutSession = {
  workoutId: string;
  workoutName: string;
  /** Backend exercise-log ID created by POST /sessions/start */
  logId: string | null;
  exercises: WorkoutExercise[];
  currentExerciseIndex: number;
  exerciseProgress: ExerciseProgress[];
  timerMode: TimerMode;
  timerState: TimerState;
  /** Timestamp (ms) when the current timer was started */
  timerStartedAt: number | null;
  /** Accumulated elapsed ms before the last pause */
  timerElapsedBeforePause: number;
  /** Rest countdown duration in seconds (set before starting rest) */
  restDurationSeconds: number;
  startedAt: string;
  /** Default rest between sets in seconds */
  defaultRestSeconds: number;
  /** Current set's rep count (editable before confirming) */
  currentReps: number;
  /** How this exercise is being tracked */
  trainingMode: TrainingMode;
  /** Target countdown seconds for timed mode */
  timedDurationSeconds: number;
};

export type WorkoutSessionSummary = {
  workoutId: string;
  workoutName: string;
  totalDurationSeconds: number;
  exercises: Array<{
    exercise: WorkoutExercise;
    sets: SetEntry[];
    totalSets: number;
    totalReps: number;
    totalExerciseTime: number;
    totalRestTime: number;
  }>;
};
