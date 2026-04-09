import { screen, userEvent } from '@testing-library/react-native';

import { PaginationControls } from '@/components/ui/pagination-controls';
import { renderWithProviders } from '@/test-utils/render';

describe('PaginationControls', () => {
  test('moves to the next and previous page when allowed', async () => {
    const onPageChange = jest.fn();
    const user = userEvent.setup();

    renderWithProviders(
      <PaginationControls
        pageNumber={2}
        totalPages={4}
        totalCount={18}
        onPageChange={onPageChange}
      />
    );

    await user.press(screen.getByRole('button', { name: 'Next' }));
    await user.press(screen.getByRole('button', { name: 'Previous' }));

    expect(onPageChange).toHaveBeenNthCalledWith(1, 3);
    expect(onPageChange).toHaveBeenNthCalledWith(2, 1);
  });

  test('disables navigation at the boundaries', () => {
    renderWithProviders(
      <PaginationControls
        pageNumber={1}
        totalPages={1}
        totalCount={3}
        onPageChange={jest.fn()}
      />
    );

    expect(screen.getByRole('button', { name: 'Previous' }).props.accessibilityState.disabled).toBe(true);
    expect(screen.getByRole('button', { name: 'Next' }).props.accessibilityState.disabled).toBe(true);
  });
});
