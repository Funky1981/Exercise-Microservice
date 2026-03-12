# Phase 2: Design System

## Goal

Create a reusable dark-theme UI foundation that looks intentional on mobile and web instead of default Expo styling.

## Decisions Already Locked

- Typography:
  - display and headings: Space Grotesk
  - body and labels: Manrope
- Style mechanism: `StyleSheet` plus typed tokens, not ad hoc inline theming.
- Visual tone: deep navy surfaces, cool mint accent, warm highlight accent.

## Files and Folders

- `src/theme/tokens.ts`
- `src/theme/navigation.ts`
- `src/components/ui/*`

## Implementation Tasks

1. Keep all colors, spacing, radii, and type families in tokens.
2. Build screen, card, button, field, and heading primitives before adding feature complexity.
3. Use large radii, layered surfaces, and restrained glow effects to keep the UI modern without depending on dark-purple defaults.
4. Maintain minimum hit targets around `44x44`.
5. Use text colors with clear hierarchy:
  - primary text
  - muted body text
  - utility labels

## Accessibility Checks

- Inputs must expose labels.
- Buttons must keep contrast against background.
- Layout must tolerate larger font scales without clipping.
- Content should stay readable at compact widths without horizontal scroll.

## Acceptance Criteria

- New screens can be assembled from existing UI primitives.
- No new feature screen should need raw color literals unless adding new tokens.

## Handoff Prompt

Implement Phase 3 by wiring route groups, tabs, guards, and loading redirects on top of the current root provider stack.
