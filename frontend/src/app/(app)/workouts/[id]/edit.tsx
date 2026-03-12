import { useLocalSearchParams } from 'expo-router';

import { WorkoutFormScreen } from '@/features/workouts/workout-form-screen';

export default function EditWorkoutRoute() {
  const params = useLocalSearchParams<{ id: string }>();

  return <WorkoutFormScreen mode="edit" workoutId={params.id} />;
}
