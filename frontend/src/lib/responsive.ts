import { useWindowDimensions } from 'react-native';

import { tokens, type Breakpoint } from '@/theme/tokens';

type ResponsiveValues<T> = {
  compact: T;
  medium?: T;
  expanded?: T;
};

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

export function pickResponsiveValue<T>(
  breakpoint: Breakpoint,
  values: ResponsiveValues<T>
): T {
  if (breakpoint === 'expanded') {
    return values.expanded ?? values.medium ?? values.compact;
  }

  if (breakpoint === 'medium') {
    return values.medium ?? values.compact;
  }

  return values.compact;
}
