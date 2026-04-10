export const queryKeys = {
  analytics: {
    summary: (userId?: string | null) => ['analytics', 'workout-summary', userId] as const,
    weekly: (userId?: string | null, weeks?: number) => ['analytics', 'weekly', userId, weeks] as const,
    exercise: (userId?: string | null, exerciseId?: string) => ['analytics', 'exercise', userId, exerciseId] as const,
  },
  users: {
    profile: (userId?: string | null) => ['users', 'profile', userId] as const,
  },
  exercises: {
    catalogue: (
      pageNumber: number,
      pageSize: number,
      filters?: Record<string, string | null | undefined>
    ) => ['exercises', 'catalogue', pageNumber, pageSize, filters ?? {}] as const,
    detail: (id: string) => ['exercises', 'detail', id] as const,
    filters: () => ['exercises', 'filters'] as const,
  },
  workouts: {
    list: (userId: string | undefined, pageNumber: number, pageSize: number) =>
      ['workouts', 'list', userId, pageNumber, pageSize] as const,
    detail: (userId: string | undefined, id: string) =>
      ['workouts', 'detail', userId, id] as const,
  },
  workoutPlans: {
    list: (userId: string | undefined, pageNumber: number, pageSize: number) =>
      ['workout-plans', 'list', userId, pageNumber, pageSize] as const,
    detail: (userId: string | undefined, id: string) =>
      ['workout-plans', 'detail', userId, id] as const,
  },
  exerciseLogs: {
    list: (userId: string | undefined, pageNumber: number, pageSize: number) =>
      ['exercise-logs', 'list', userId, pageNumber, pageSize] as const,
    detail: (userId: string | undefined, id: string) =>
      ['exercise-logs', 'detail', userId, id] as const,
  },
};
