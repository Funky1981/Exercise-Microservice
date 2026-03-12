import { Stack } from 'expo-router';

import { tokens } from '@/theme/tokens';

export default function AppLayout() {
  return (
    <Stack
      screenOptions={{
        headerStyle: {
          backgroundColor: tokens.colors.surface,
        },
        headerShadowVisible: false,
        headerTintColor: tokens.colors.text,
        headerTitleStyle: {
          fontFamily: tokens.typography.heading,
        },
        contentStyle: {
          backgroundColor: tokens.colors.canvas,
        },
      }}>
      <Stack.Screen name="(tabs)" options={{ headerShown: false }} />
      <Stack.Screen name="exercises/[id]" options={{ title: 'Exercise detail' }} />
      <Stack.Screen name="workouts/[id]" options={{ title: 'Workout detail' }} />
      <Stack.Screen
        name="workouts/new"
        options={{ title: 'New workout', presentation: 'modal' }}
      />
      <Stack.Screen
        name="workouts/[id]/edit"
        options={{ title: 'Edit workout', presentation: 'modal' }}
      />
      <Stack.Screen name="plans/index" options={{ title: 'Workout plans' }} />
      <Stack.Screen name="plans/[id]" options={{ title: 'Plan detail' }} />
      <Stack.Screen
        name="plans/new"
        options={{ title: 'New plan', presentation: 'modal' }}
      />
      <Stack.Screen
        name="plans/[id]/edit"
        options={{ title: 'Edit plan', presentation: 'modal' }}
      />
      <Stack.Screen name="logs/index" options={{ title: 'Exercise logs' }} />
      <Stack.Screen name="logs/[id]" options={{ title: 'Log detail' }} />
      <Stack.Screen
        name="logs/new"
        options={{ title: 'New log', presentation: 'modal' }}
      />
    </Stack>
  );
}
