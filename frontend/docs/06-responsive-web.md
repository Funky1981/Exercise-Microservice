# Phase 6: Responsive and Web

## Goal

Make the same app feel deliberate on compact phones, tablets, and desktop web.

## Decisions Already Locked

- Layout changes follow breakpoint buckets, not device-name checks.
- `useWindowDimensions` is the default signal for responsive behavior.
- Avoid fixed heights and widths for major panels.

## Files and Folders

- `src/lib/responsive.ts`
- `src/components/ui/app-screen.tsx`
- feature screen layout files

## Implementation Tasks

1. Let compact screens stack vertically.
2. Let medium and expanded screens use multi-column sections where it improves scanability.
3. Use `FlatList` for larger collections instead of `ScrollView` with mapped children.
4. Keep max content width constrained on desktop web.
5. Preserve safe-area padding and avoid content hiding under tab bars.

## Acceptance Criteria

- Compact width remains single-column and touch-friendly.
- Expanded width uses the extra space for structure, not just larger gaps.
- No screen depends on a single OS-specific layout assumption.

## Handoff Prompt

Implement Phase 7 by locking in tests, CI checks, and release mechanics for Expo/EAS.
