import { useLocalSearchParams } from 'expo-router';

import { ExerciseDetailScreen } from '@/features/exercises/exercise-detail-screen';

export default function ExerciseDetailRoute() {
  const params = useLocalSearchParams<{ id: string }>();

  return <ExerciseDetailScreen exerciseId={params.id} />;
}
