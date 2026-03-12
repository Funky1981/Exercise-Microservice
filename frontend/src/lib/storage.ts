import AsyncStorage from '@react-native-async-storage/async-storage';
import * as SecureStore from 'expo-secure-store';
import { Platform } from 'react-native';

const memoryStore = new Map<string, string>();

function canUseLocalStorage() {
  return typeof window !== 'undefined' && typeof window.localStorage !== 'undefined';
}

export const appStorage = {
  async getItem(key: string) {
    if (Platform.OS === 'web') {
      if (canUseLocalStorage()) {
        return window.localStorage.getItem(key);
      }

      return memoryStore.get(key) ?? null;
    }

    return SecureStore.getItemAsync(key);
  },

  async setItem(key: string, value: string) {
    if (Platform.OS === 'web') {
      if (canUseLocalStorage()) {
        window.localStorage.setItem(key, value);
        return;
      }

      memoryStore.set(key, value);
      return;
    }

    await SecureStore.setItemAsync(key, value);
  },

  async removeItem(key: string) {
    if (Platform.OS === 'web') {
      if (canUseLocalStorage()) {
        window.localStorage.removeItem(key);
        return;
      }

      memoryStore.delete(key);
      return;
    }

    await SecureStore.deleteItemAsync(key);
  },
};

export { AsyncStorage };
