import AsyncStorage from '@react-native-async-storage/async-storage';

import type { WorkoutSession } from './types';

const SESSION_KEY = 'workout-session:active';

export const sessionStorage = {
  async save(session: WorkoutSession): Promise<void> {
    await AsyncStorage.setItem(SESSION_KEY, JSON.stringify(session));
  },

  async load(): Promise<WorkoutSession | null> {
    const raw = await AsyncStorage.getItem(SESSION_KEY);
    if (!raw) return null;

    try {
      return JSON.parse(raw) as WorkoutSession;
    } catch {
      await AsyncStorage.removeItem(SESSION_KEY);
      return null;
    }
  },

  async clear(): Promise<void> {
    await AsyncStorage.removeItem(SESSION_KEY);
  },
};
