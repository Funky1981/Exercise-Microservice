export type Session = {
  token: string;
  refreshToken: string;
  expiresAt: string;
  refreshTokenExpiry: string;
  userId: string;
  name: string;
  email: string;
};

export type LoginPayload = {
  email: string;
  password: string;
};

export type RegisterPayload = {
  name: string;
  email: string;
  password: string;
};

export type LoginResponse = {
  token: string;
  expiresAt: string;
  userId: string;
  name: string;
  email: string;
  refreshToken: string;
  refreshTokenExpiry: string;
};

export type ProblemDetails = {
  title?: string;
  detail?: string;
  status?: number;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};

export type ExerciseRegion = 'upper-body' | 'lower-body' | 'core' | 'cardio' | 'other';

export type Exercise = {
  id: string;
  externalId?: string | null;
  sourceProvider?: string | null;
  name: string;
  bodyPart: string;
  equipment?: string | null;
  targetMuscle: string;
  gifUrl?: string | null;
  secondaryMuscles?: string[];
  instructions?: string[];
  sourcePayloadJson?: string | null;
  description?: string | null;
  difficulty?: string | null;
  category?: string | null;
};

export type ExerciseFilters = {
  region?: ExerciseRegion | null;
  bodyPart?: string | null;
  equipment?: string | null;
  search?: string | null;
};

export type ExerciseFilterOptions = {
  regions: ExerciseRegion[];
  bodyPartsByRegion: Record<string, string[]>;
  equipment: string[];
};

export type Workout = {
  id: string;
  userId: string;
  name?: string | null;
  date: string;
  hasExplicitTime: boolean;
  duration?: string | null;
  notes?: string | null;
  isCompleted: boolean;
  exercises: WorkoutExercise[];
};

export type WorkoutPlan = {
  id: string;
  userId: string;
  name?: string | null;
  startDate: string;
  endDate?: string | null;
  notes?: string | null;
  isActive: boolean;
  workouts: WorkoutPlanWorkout[];
};

export type WorkoutExercise = {
  id: string;
  exerciseId: string;
  name: string;
  bodyPart: string;
  targetMuscle: string;
  equipment?: string | null;
  sets: number;
  reps: number;
  restSeconds: number;
  order: number;
};

export type WorkoutPlanWorkout = {
  id: string;
  name?: string | null;
  date: string;
  duration?: string | null;
  isCompleted: boolean;
  exerciseIds: string[];
};

export type ExerciseLogEntry = {
  exerciseId: string;
  sets: number;
  reps: number;
  duration?: string | null;
  restTime?: string | null;
  completedAt?: string | null;
};

export type ExerciseLog = {
  id: string;
  userId: string;
  workoutId?: string | null;
  name?: string | null;
  date: string;
  duration?: string | null;
  notes?: string | null;
  isCompleted: boolean;
  entries: ExerciseLogEntry[];
};

export type CreateWorkoutPayload = {
  name?: string;
  date: string;
  hasExplicitTime: boolean;
  notes?: string;
  exerciseIds: string[];
};

export type UpdateWorkoutPayload = CreateWorkoutPayload;

export type CreateWorkoutPlanPayload = {
  name?: string;
  startDate: string;
  endDate?: string | null;
  notes?: string;
};

export type UpdateWorkoutPlanPayload = CreateWorkoutPlanPayload;

export type CreateExerciseLogPayload = {
  name?: string;
  date: string;
  notes?: string;
};

export type AddExerciseLogEntryPayload = {
  exerciseId: string;
  sets: number;
  reps: number;
  duration?: string | null;
};

export type AddWorkoutExercisePayload = {
  exerciseId: string;
};

export type UpdatePrescriptionPayload = {
  sets: number;
  reps: number;
  restSeconds: number;
};

export type AddWorkoutPlanWorkoutPayload = {
  workoutId: string;
};

export type WorkoutSummary = {
  userId: string;
  totalWorkouts: number;
  completedWorkouts: number;
  totalWorkoutDuration: string;
  totalExerciseLogs: number;
  completedExerciseLogs: number;
  totalExerciseLogDuration: string;
};

export type WeeklyDataPoint = {
  weekStart: string;
  workoutCount: number;
  totalSets: number;
  totalReps: number;
  volume: number;
  durationSeconds: number;
  avgRestSeconds: number;
};

export type WeeklyAnalytics = {
  weeks: WeeklyDataPoint[];
  avgWorkoutsPerWeek: number;
  totalVolume: number;
  totalDuration: string;
  avgRestSeconds: number;
};

export type ExerciseDataPoint = {
  date: string;
  sets: number;
  reps: number;
  volume: number;
  durationSeconds: number;
  restSeconds: number;
};

export type ExerciseAnalytics = {
  exerciseId: string;
  exerciseName: string;
  totalSets: number;
  totalReps: number;
  totalVolume: number;
  avgRepsPerSet: number;
  avgRestSeconds: number;
  totalDuration: string;
  dataPoints: ExerciseDataPoint[];
};

export type UserProfile = {
  id: string;
  name: string;
  email: string;
  userName?: string | null;
  provider?: string | null;
  heightCm?: number | null;
  weightKg?: number | null;
  createdAt: string;
};

export type UpdateProfilePayload = {
  userName?: string | null;
  heightCm?: number | null;
  weightKg?: number | null;
};
