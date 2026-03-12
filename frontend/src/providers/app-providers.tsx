import { Manrope_400Regular, Manrope_600SemiBold, Manrope_700Bold } from '@expo-google-fonts/manrope';
import { SpaceGrotesk_500Medium, SpaceGrotesk_700Bold } from '@expo-google-fonts/space-grotesk';
import { ThemeProvider } from '@react-navigation/native';
import { PersistQueryClientProvider } from '@tanstack/react-query-persist-client';
import { createAsyncStoragePersister } from '@tanstack/query-async-storage-persister';
import { useFonts } from 'expo-font';
import * as SplashScreen from 'expo-splash-screen';
import * as SystemUI from 'expo-system-ui';
import { useEffect, useState } from 'react';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { SafeAreaProvider } from 'react-native-safe-area-context';

import { AsyncStorage } from '@/lib/storage';
import { createQueryClient } from '@/lib/query-client';
import { useReactQueryFocusSync } from '@/lib/query-focus';
import { SessionProvider } from '@/state/session-context';
import { navigationTheme } from '@/theme/navigation';
import { tokens } from '@/theme/tokens';

SplashScreen.preventAutoHideAsync().catch(() => {});

const queryClient = createQueryClient();
const persister = createAsyncStoragePersister({
  storage: AsyncStorage,
});

export function AppProviders({ children }: React.PropsWithChildren) {
  const [mounted, setMounted] = useState(false);
  const [loaded] = useFonts({
    Manrope_400Regular,
    Manrope_600SemiBold,
    Manrope_700Bold,
    SpaceGrotesk_500Medium,
    SpaceGrotesk_700Bold,
  });

  useReactQueryFocusSync();

  useEffect(() => {
    void SystemUI.setBackgroundColorAsync(tokens.colors.canvas);
  }, []);

  useEffect(() => {
    if (loaded) {
      setMounted(true);
      void SplashScreen.hideAsync();
    }
  }, [loaded]);

  if (!mounted || !loaded) {
    return null;
  }

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <SafeAreaProvider>
        <PersistQueryClientProvider
          client={queryClient}
          persistOptions={{ persister }}>
          <ThemeProvider value={navigationTheme}>
            <SessionProvider>{children}</SessionProvider>
          </ThemeProvider>
        </PersistQueryClientProvider>
      </SafeAreaProvider>
    </GestureHandlerRootView>
  );
}
