import { env } from '@/lib/env';
import type {
  Exercise,
  LoginPayload,
  LoginResponse,
  PagedResult,
  ProblemDetails,
  RegisterPayload,
  Session,
  Workout,
  WorkoutSummary,
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

  async getWorkouts(pageNumber = 1, pageSize = 20) {
    return request<PagedResult<Workout>>(
      `/api/workouts?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  },

  async getWorkoutSummary() {
    return request<WorkoutSummary>('/api/analytics/workout-summary');
  },
};
