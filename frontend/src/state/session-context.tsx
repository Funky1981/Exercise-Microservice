import { useQueryClient } from '@tanstack/react-query';
import { router } from 'expo-router';
import React, { createContext, useContext, useEffect, useMemo, useRef, useState } from 'react';

import { apiClient, bindSessionAccessors } from '@/api/client';
import type { LoginPayload, RegisterPayload, Session } from '@/api/types';
import { appStorage } from '@/lib/storage';

const SESSION_KEY = 'exercise.session';

type SessionStatus = 'booting' | 'anonymous' | 'authenticated';

type SessionContextValue = {
  status: SessionStatus;
  session: Session | null;
  signIn: (payload: LoginPayload) => Promise<void>;
  register: (payload: RegisterPayload) => Promise<void>;
  signOut: () => Promise<void>;
};

const SessionContext = createContext<SessionContextValue | null>(null);

export function SessionProvider({ children }: React.PropsWithChildren) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState<SessionStatus>('booting');
  const [session, setSession] = useState<Session | null>(null);

  useEffect(() => {
    async function bootstrap() {
      const raw = await appStorage.getItem(SESSION_KEY);

      if (!raw) {
        setStatus('anonymous');
        return;
      }

      try {
        const parsed = JSON.parse(raw) as Session;
        setSession(parsed);
        setStatus('authenticated');
      } catch {
        await appStorage.removeItem(SESSION_KEY);
        setStatus('anonymous');
      }
    }

    void bootstrap();
  }, []);

  const signOut = React.useCallback(async () => {
    setSession(null);
    setStatus('anonymous');
    await appStorage.removeItem(SESSION_KEY);
    queryClient.clear();
    router.replace('/(auth)/sign-in');
  }, [queryClient]);

  const signIn = React.useCallback(async (payload: LoginPayload) => {
    const nextSession = await apiClient.login(payload);
    setSession(nextSession);
    setStatus('authenticated');
    await appStorage.setItem(SESSION_KEY, JSON.stringify(nextSession));
    router.replace('/(app)/(tabs)');
  }, []);

  const value = useMemo<SessionContextValue>(
    () => ({
      status,
      session,
      signIn,
      async register(payload) {
        await apiClient.register(payload);
        await signIn({
          email: payload.email,
          password: payload.password,
        });
      },
      signOut,
    }),
    [session, signIn, signOut, status]
  );

  const valueRef = useRef(value);
  valueRef.current = value;

  useEffect(() => {
    bindSessionAccessors({
      getSession: () => valueRef.current.session,
      saveSession: async (nextSession) => {
        setSession(nextSession);
        setStatus('authenticated');
        await appStorage.setItem(SESSION_KEY, JSON.stringify(nextSession));
      },
      clearSession: async () => {
        await signOut();
      },
    });
  }, [signOut]);

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const context = useContext(SessionContext);

  if (!context) {
    throw new Error('useSession must be used within a SessionProvider');
  }

  return context;
}
