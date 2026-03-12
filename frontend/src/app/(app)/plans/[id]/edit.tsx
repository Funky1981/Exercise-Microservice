import { useLocalSearchParams } from 'expo-router';

import { WorkoutPlanFormScreen } from '@/features/workout-plans/workout-plan-form-screen';

export default function EditWorkoutPlanRoute() {
  const params = useLocalSearchParams<{ id: string }>();

  return <WorkoutPlanFormScreen mode="edit" planId={params.id} />;
}
