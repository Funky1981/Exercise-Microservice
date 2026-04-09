import { screen, userEvent, waitFor } from '@testing-library/react-native';
import { Text } from 'react-native';

import { apiClient } from '@/api/client';
import { SessionProvider, useSession } from '@/state/session-context';
import { renderWithProviders } from '@/test-utils/render';

jest.mock('@/lib/storage', () => ({
  appStorage: {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
  },
}));

const { appStorage } = jest.requireMock('@/lib/storage') as {
  appStorage: {
    getItem: jest.Mock;
    setItem: jest.Mock;
    removeItem: jest.Mock;
  };
};

jest.mock('@/api/client', () => {
  const actual = jest.requireActual('@/api/client');

  return {
    ...actual,
    apiClient: {
      ...actual.apiClient,
      login: jest.fn(),
      register: jest.fn(),
    },
  };
});

function SessionHarness() {
  const { status, session, signIn, signOut } = useSession();

  return (
    <>
      <Text testID="status">{status}</Text>
      <Text testID="email">{session?.email ?? 'none'}</Text>
      <Text accessibilityRole="button" onPress={() => void signIn({ email: 'user@example.com', password: 'secret123' })}>
        Trigger sign in
      </Text>
      <Text accessibilityRole="button" onPress={() => void signOut()}>
        Trigger sign out
      </Text>
    </>
  );
}

describe('SessionProvider', () => {
  beforeEach(() => {
    appStorage.getItem.mockReset();
    appStorage.setItem.mockReset();
    appStorage.removeItem.mockReset();
    (apiClient.login as jest.Mock).mockReset();
    (apiClient.register as jest.Mock).mockReset();
  });

  test('bootstraps to anonymous when no session is stored', async () => {
    appStorage.getItem.mockResolvedValue(null);

    renderWithProviders(
      <SessionProvider>
        <SessionHarness />
      </SessionProvider>
    );

    await waitFor(() => {
      expect(appStorage.getItem).toHaveBeenCalledWith('exercise.session');
    });
    await waitFor(() => {
      expect(screen.getByText('anonymous')).toBeTruthy();
    });
    expect(screen.getByText('none')).toBeTruthy();
  });

  test('signs in, persists the session, and signs out cleanly', async () => {
    appStorage.getItem.mockResolvedValue(null);
    (apiClient.login as jest.Mock).mockResolvedValue({
      token: 'token-1',
      refreshToken: 'refresh-1',
      expiresAt: '2026-03-12T10:00:00Z',
      refreshTokenExpiry: '2026-03-13T10:00:00Z',
      userId: 'user-1',
      name: 'Test User',
      email: 'user@example.com',
    });

    const user = userEvent.setup();

    renderWithProviders(
      <SessionProvider>
        <SessionHarness />
      </SessionProvider>
    );

    await waitFor(() => {
      expect(appStorage.getItem).toHaveBeenCalledWith('exercise.session');
    });
    await waitFor(() => {
      expect(screen.getByText('anonymous')).toBeTruthy();
    });

    await user.press(screen.getByRole('button', { name: 'Trigger sign in' }));

    await waitFor(() => {
      expect(screen.getByText('authenticated')).toBeTruthy();
    });
    expect(appStorage.setItem).toHaveBeenCalledWith(
      'exercise.session',
      expect.stringContaining('user@example.com')
    );

    await user.press(screen.getByRole('button', { name: 'Trigger sign out' }));

    await waitFor(() => {
      expect(screen.getByText('anonymous')).toBeTruthy();
    });
    expect(appStorage.removeItem).toHaveBeenCalledWith('exercise.session');
  });
});
