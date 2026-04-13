import { useLocalSearchParams } from 'expo-router';

import { WorkoutSessionScreen } from '@/features/workout-session/workout-session-screen';

export default function WorkoutSessionRoute() {
  const { workoutId, fresh } = useLocalSearchParams<{ workoutId?: string; fresh?: string }>();
  return <WorkoutSessionScreen workoutId={workoutId} freshStart={fresh === '1'} />;
}
