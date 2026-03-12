import { useLocalSearchParams } from 'expo-router';

import { WorkoutPlanDetailScreen } from '@/features/workout-plans/workout-plan-detail-screen';

export default function WorkoutPlanDetailRoute() {
  const params = useLocalSearchParams<{ id: string }>();

  return <WorkoutPlanDetailScreen planId={params.id} />;
}
