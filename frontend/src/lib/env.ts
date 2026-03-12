import { Platform } from 'react-native';

const fallbackBaseUrl = Platform.select({
  android: 'http://10.0.2.2:5034',
  default: 'http://localhost:5034',
  ios: 'http://localhost:5034',
  web: 'http://localhost:5034',
});

export const env = {
  apiBaseUrl: process.env.EXPO_PUBLIC_API_BASE_URL ?? fallbackBaseUrl ?? 'http://localhost:5034',
};
