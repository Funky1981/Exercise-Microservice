import { useWindowDimensions } from 'react-native';

import { tokens, type Breakpoint } from '@/theme/tokens';

export function getBreakpoint(width: number): Breakpoint {
  if (width <= tokens.layout.compactMaxWidth) {
    return 'compact';
  }

  if (width <= tokens.layout.mediumMaxWidth) {
    return 'medium';
  }

  return 'expanded';
}

export function useBreakpoint() {
  const { width, height, fontScale } = useWindowDimensions();
  const breakpoint = getBreakpoint(width);

  return {
    width,
    height,
    fontScale,
    breakpoint,
    isCompact: breakpoint === 'compact',
    isMedium: breakpoint === 'medium',
    isExpanded: breakpoint === 'expanded',
  };
}
