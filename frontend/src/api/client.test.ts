import { apiClient, bindSessionAccessors } from '@/api/client';
import type { Session } from '@/api/types';

const baseSession: Session = {
  token: 'expired-token',
  refreshToken: 'refresh-token',
  expiresAt: '2026-03-12T10:00:00Z',
  refreshTokenExpiry: '2026-03-13T10:00:00Z',
  userId: 'user-1',
  name: 'Tester',
  email: 'tester@example.com',
};

describe('apiClient', () => {
  beforeEach(() => {
    bindSessionAccessors({
      getSession: () => baseSession,
      saveSession: jest.fn().mockResolvedValue(undefined),
      clearSession: jest.fn().mockResolvedValue(undefined),
    });
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  test('retries an authenticated request after refreshing the session', async () => {
    const saveSession = jest.fn().mockResolvedValue(undefined);
    bindSessionAccessors({
      getSession: () => baseSession,
      saveSession,
      clearSession: jest.fn().mockResolvedValue(undefined),
    });

    const fetchMock = global.fetch as jest.Mock;
    fetchMock
      .mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ detail: 'Token expired' }),
      })
      .mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({
          token: 'fresh-token',
          refreshToken: 'fresh-refresh-token',
          expiresAt: '2026-03-12T11:00:00Z',
          refreshTokenExpiry: '2026-03-13T11:00:00Z',
          userId: 'user-1',
          name: 'Tester',
          email: 'tester@example.com',
        }),
      })
      .mockResolvedValueOnce({
        ok: true,
        status: 200,
        json: async () => ({
          items: [],
          totalCount: 0,
          pageNumber: 1,
          pageSize: 20,
          totalPages: 0,
          hasPreviousPage: false,
          hasNextPage: false,
        }),
      });

    await apiClient.getExercises();

    expect(fetchMock).toHaveBeenCalledTimes(3);
    expect(fetchMock.mock.calls[0]?.[0]).toContain('/api/exercises');
    expect(fetchMock.mock.calls[1]?.[0]).toContain('/api/auth/refresh');
    expect(fetchMock.mock.calls[2]?.[0]).toContain('/api/exercises');
    expect(saveSession).toHaveBeenCalledWith(
      expect.objectContaining({
        token: 'fresh-token',
        refreshToken: 'fresh-refresh-token',
      })
    );
  });

  test('clears the session when refresh fails', async () => {
    const clearSession = jest.fn().mockResolvedValue(undefined);
    bindSessionAccessors({
      getSession: () => baseSession,
      saveSession: jest.fn().mockResolvedValue(undefined),
      clearSession,
    });

    const fetchMock = global.fetch as jest.Mock;
    fetchMock
      .mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ detail: 'Token expired' }),
      })
      .mockResolvedValueOnce({
        ok: false,
        status: 401,
        json: async () => ({ detail: 'Refresh failed' }),
      });

    await expect(apiClient.getExercises()).rejects.toThrow('Refresh failed');
    expect(clearSession).toHaveBeenCalledTimes(1);
  });
});
