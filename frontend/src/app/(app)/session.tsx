import { useLocalSearchParams } from 'expo-router';

import { WorkoutSessionScreen } from '@/features/workout-session/workout-session-screen';

export default function WorkoutSessionRoute() {
  const { workoutId } = useLocalSearchParams<{ workoutId?: string }>();
  return <WorkoutSessionScreen workoutId={workoutId} />;
}
