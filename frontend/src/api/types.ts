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

export type Exercise = {
  id: string;
  name: string;
  bodyPart: string;
  equipment?: string | null;
  targetMuscle: string;
  gifUrl?: string | null;
  description?: string | null;
  difficulty?: string | null;
};

export type Workout = {
  id: string;
  userId: string;
  name?: string | null;
  date: string;
  duration?: string | null;
  notes?: string | null;
  isCompleted: boolean;
};

export type WorkoutPlan = {
  id: string;
  userId: string;
  name?: string | null;
  startDate: string;
  endDate?: string | null;
  notes?: string | null;
  isActive: boolean;
};

export type ExerciseLogEntry = {
  exerciseId: string;
  sets: number;
  reps: number;
  duration?: string | null;
};

export type ExerciseLog = {
  id: string;
  userId: string;
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
  notes?: string;
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

export type WorkoutSummary = {
  userId: string;
  totalWorkouts: number;
  completedWorkouts: number;
  totalWorkoutDuration: string;
  totalExerciseLogs: number;
  completedExerciseLogs: number;
  totalExerciseLogDuration: string;
};
