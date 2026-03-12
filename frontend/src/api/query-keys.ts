export const queryKeys = {
  analytics: {
    summary: (userId?: string | null) => ['analytics', 'workout-summary', userId] as const,
  },
  exercises: {
    catalogue: (pageNumber: number, pageSize: number) =>
      ['exercises', 'catalogue', pageNumber, pageSize] as const,
    detail: (id: string) => ['exercises', 'detail', id] as const,
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
