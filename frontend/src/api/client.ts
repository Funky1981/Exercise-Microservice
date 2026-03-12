import { env } from '@/lib/env';
import type {
  AddWorkoutExercisePayload,
  AddWorkoutPlanWorkoutPayload,
  AddExerciseLogEntryPayload,
  CreateExerciseLogPayload,
  CreateWorkoutPayload,
  CreateWorkoutPlanPayload,
  Exercise,
  ExerciseLog,
  LoginPayload,
  LoginResponse,
  PagedResult,
  ProblemDetails,
  RegisterPayload,
  Session,
  Workout,
  WorkoutPlan,
  WorkoutSummary,
  UpdateWorkoutPayload,
  UpdateWorkoutPlanPayload,
} from '@/api/types';

type SessionAccessors = {
  getSession: () => Session | null;
  saveSession: (session: Session) => Promise<void>;
  clearSession: () => Promise<void>;
};

let sessionAccessors: SessionAccessors = {
  getSession: () => null,
  saveSession: async () => {},
  clearSession: async () => {},
};

export function bindSessionAccessors(accessors: SessionAccessors) {
  sessionAccessors = accessors;
}

async function parseResponse<T>(response: Response) {
  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function parseError(response: Response) {
  let detail = `Request failed with status ${response.status}`;

  try {
    const payload = (await response.json()) as ProblemDetails;
    detail = payload.detail ?? payload.title ?? detail;
  } catch {
    // Ignore non-JSON responses and fall back to a generic error.
  }

  throw new Error(detail);
}

function toSession(response: LoginResponse): Session {
  return {
    token: response.token,
    refreshToken: response.refreshToken,
    expiresAt: response.expiresAt,
    refreshTokenExpiry: response.refreshTokenExpiry,
    userId: response.userId,
    name: response.name,
    email: response.email,
  };
}

async function refreshSession(session: Session) {
  const response = await fetch(`${env.apiBaseUrl}/api/auth/refresh`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      email: session.email,
      refreshToken: session.refreshToken,
    }),
  });

  if (!response.ok) {
    await sessionAccessors.clearSession();
    await parseError(response);
  }

  const nextSession = toSession(await parseResponse<LoginResponse>(response));
  await sessionAccessors.saveSession(nextSession);
  return nextSession;
}

type RequestOptions = {
  auth?: boolean;
  retryOnUnauthorized?: boolean;
};

async function request<T>(
  path: string,
  init?: RequestInit,
  options: RequestOptions = {}
): Promise<T> {
  const auth = options.auth ?? true;
  const retryOnUnauthorized = options.retryOnUnauthorized ?? true;
  const session = sessionAccessors.getSession();
  const headers = new Headers(init?.headers);

  headers.set('Content-Type', 'application/json');

  if (auth && session?.token) {
    headers.set('Authorization', `Bearer ${session.token}`);
  }

  const response = await fetch(`${env.apiBaseUrl}${path}`, {
    ...init,
    headers,
  });

  if (response.status === 401 && auth && retryOnUnauthorized && session?.refreshToken) {
    await refreshSession(session);
    return request<T>(path, init, {
      auth: true,
      retryOnUnauthorized: false,
    });
  }

  if (!response.ok) {
    await parseError(response);
  }

  return parseResponse<T>(response);
}

export const apiClient = {
  async login(payload: LoginPayload) {
    const response = await request<LoginResponse>(
      '/api/auth/login',
      {
        method: 'POST',
        body: JSON.stringify(payload),
      },
      { auth: false, retryOnUnauthorized: false }
    );

    return toSession(response);
  },

  async register(payload: RegisterPayload) {
    return request<{ id: string }>(
      '/api/users/register',
      {
        method: 'POST',
        body: JSON.stringify(payload),
      },
      { auth: false, retryOnUnauthorized: false }
    );
  },

  async getExercises(pageNumber = 1, pageSize = 20) {
    return request<PagedResult<Exercise>>(
      `/api/exercises?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  },

  async getExerciseById(id: string) {
    return request<Exercise>(`/api/exercises/${id}`);
  },

  async getWorkouts(pageNumber = 1, pageSize = 20) {
    return request<PagedResult<Workout>>(
      `/api/workouts?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  },

  async getWorkoutById(id: string) {
    return request<Workout>(`/api/workouts/${id}`);
  },

  async createWorkout(payload: CreateWorkoutPayload) {
    return request<{ id: string }>('/api/workouts', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  async updateWorkout(id: string, payload: UpdateWorkoutPayload) {
    return request<void>(`/api/workouts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  async completeWorkout(id: string, duration: string) {
    return request<void>(`/api/workouts/${id}/complete`, {
      method: 'POST',
      body: JSON.stringify({ duration }),
    });
  },

  async deleteWorkout(id: string) {
    return request<void>(`/api/workouts/${id}`, {
      method: 'DELETE',
    });
  },

  async addWorkoutExercise(id: string, payload: AddWorkoutExercisePayload) {
    return request<void>(`/api/workouts/${id}/exercises`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  async removeWorkoutExercise(id: string, exerciseId: string) {
    return request<void>(`/api/workouts/${id}/exercises/${exerciseId}`, {
      method: 'DELETE',
    });
  },

  async getWorkoutPlans(pageNumber = 1, pageSize = 20) {
    return request<PagedResult<WorkoutPlan>>(
      `/api/workout-plans?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  },

  async getWorkoutPlanById(id: string) {
    return request<WorkoutPlan>(`/api/workout-plans/${id}`);
  },

  async createWorkoutPlan(payload: CreateWorkoutPlanPayload) {
    return request<{ id: string }>('/api/workout-plans', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  async updateWorkoutPlan(id: string, payload: UpdateWorkoutPlanPayload) {
    return request<void>(`/api/workout-plans/${id}`, {
      method: 'PUT',
      body: JSON.stringify(payload),
    });
  },

  async activateWorkoutPlan(id: string) {
    return request<void>(`/api/workout-plans/${id}/activate`, {
      method: 'POST',
    });
  },

  async deleteWorkoutPlan(id: string) {
    return request<void>(`/api/workout-plans/${id}`, {
      method: 'DELETE',
    });
  },

  async addWorkoutToPlan(id: string, payload: AddWorkoutPlanWorkoutPayload) {
    return request<void>(`/api/workout-plans/${id}/workouts`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  async removeWorkoutFromPlan(id: string, workoutId: string) {
    return request<void>(`/api/workout-plans/${id}/workouts/${workoutId}`, {
      method: 'DELETE',
    });
  },

  async getExerciseLogs(pageNumber = 1, pageSize = 20) {
    return request<PagedResult<ExerciseLog>>(
      `/api/exercise-logs?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  },

  async getExerciseLogById(id: string) {
    return request<ExerciseLog>(`/api/exercise-logs/${id}`);
  },

  async createExerciseLog(payload: CreateExerciseLogPayload) {
    return request<{ id: string }>('/api/exercise-logs', {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  async addExerciseLogEntry(id: string, payload: AddExerciseLogEntryPayload) {
    return request<void>(`/api/exercise-logs/${id}/entries`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });
  },

  async completeExerciseLog(id: string, totalDuration?: string | null) {
    return request<void>(`/api/exercise-logs/${id}/complete`, {
      method: 'POST',
      body: JSON.stringify({ totalDuration }),
    });
  },

  async deleteExerciseLog(id: string) {
    return request<void>(`/api/exercise-logs/${id}`, {
      method: 'DELETE',
    });
  },

  async getWorkoutSummary() {
    return request<WorkoutSummary>('/api/analytics/workout-summary');
  },
};
