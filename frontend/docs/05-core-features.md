# Phase 5: Core Features

## Goal

Turn the foundation screens into a usable MVP for the existing backend.

## Feature Order

1. Auth
2. Dashboard analytics
3. Exercise catalogue and exercise detail
4. Workouts CRUD and completion
5. Workout plans CRUD, activation, add/remove workout
6. Exercise logs CRUD, add entry, completion

## Files and Folders

- `src/features/dashboard/*`
- `src/features/exercises/*`
- `src/features/workouts/*`
- `src/features/workout-plans/*`
- `src/features/exercise-logs/*`

## Implementation Tasks

1. Keep queries, screens, and helpers grouped by feature.
2. Add detail routes above tabs for view/edit flows.
3. Prefer optimistic UI only after the backend contract is stable and query invalidation rules are explicit.
4. Reuse shared cards, fields, and screen shells instead of inventing screen-local components.

## Acceptance Criteria

- A user can authenticate, browse exercises, view workout summary data, and manage workouts from the app.
- All feature routes work on phone and web layouts.

## Handoff Prompt

Implement Phase 6 by improving tablet and desktop behavior without forking the whole UI.
