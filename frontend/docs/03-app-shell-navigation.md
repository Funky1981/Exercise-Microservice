# Phase 3: App Shell and Navigation

## Goal

Define a route structure that scales from MVP to a larger product without route sprawl.

## Decisions Already Locked

- Public routes live in `(auth)`.
- Authenticated routes live in `(app)`.
- Primary navigation lives in `(app)/(tabs)`.
- Root `index.tsx` performs auth-aware redirect logic.

## Files and Folders

- `src/app/_layout.tsx`
- `src/app/index.tsx`
- `src/app/(auth)/*`
- `src/app/(app)/*`

## Implementation Tasks

1. Keep root layout provider-only.
2. Keep auth layout and app layout thin.
3. Put durable top-level destinations into tabs:
  - dashboard
  - exercises
  - workouts
  - profile
4. Reserve stacks above tabs for detail and modal routes later.
5. Keep route guards driven by session status, not screen-specific checks.

## Acceptance Criteria

- Anonymous users land in sign-in.
- Authenticated users land in tabs.
- The route tree is clear enough for AI or human contributors to extend safely.

## Handoff Prompt

Implement Phase 4 by hardening auth, API access, query keys, refresh handling, and logout cache clearing.
