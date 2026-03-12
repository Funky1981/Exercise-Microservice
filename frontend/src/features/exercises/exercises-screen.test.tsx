import { fireEvent, screen, waitFor } from '@testing-library/react-native';

import { apiClient } from '@/api/client';
import { ExercisesScreen } from '@/features/exercises/exercises-screen';
import { renderWithProviders } from '@/test-utils/render';

jest.mock('@/api/client', () => ({
  apiClient: {
    getExercises: jest.fn(),
  },
}));

describe('ExercisesScreen', () => {
  beforeEach(() => {
    (apiClient.getExercises as jest.Mock).mockReset();
  });

  test('renders exercise results and filters the current page', async () => {
    (apiClient.getExercises as jest.Mock).mockResolvedValue({
      items: [
        {
          id: '1',
          name: 'Bench Press',
          bodyPart: 'Chest',
          targetMuscle: 'Pectorals',
          equipment: 'Barbell',
          description: 'Pressing movement',
          difficulty: 'Intermediate',
        },
        {
          id: '2',
          name: 'Back Squat',
          bodyPart: 'Legs',
          targetMuscle: 'Quadriceps',
          equipment: 'Barbell',
          description: 'Squat movement',
          difficulty: 'Intermediate',
        },
      ],
      totalCount: 2,
      pageNumber: 1,
      pageSize: 12,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    });

    renderWithProviders(<ExercisesScreen />);

    expect(await screen.findByText('Bench Press')).toBeTruthy();
    expect(screen.getByText('Back Squat')).toBeTruthy();

    fireEvent.changeText(screen.getByLabelText('Search'), 'bench');

    await waitFor(() => {
      expect(screen.getByText('Bench Press')).toBeTruthy();
      expect(screen.queryByText('Back Squat')).toBeNull();
    });
  });
});
