import { useLocalSearchParams } from 'expo-router';

import { WorkoutFormScreen } from '@/features/workouts/workout-form-screen';

export default function NewWorkoutRoute() {
  const params = useLocalSearchParams<{ duplicateWorkoutId?: string }>();

  return <WorkoutFormScreen mode="create" duplicateWorkoutId={params.duplicateWorkoutId} />;
}
