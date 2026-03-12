# Phase 7: Testing and Release

## Goal

Define the quality gate for the frontend before feature velocity increases.

## Recommended Test Layers

- Type safety: `tsc --noEmit`
- Linting: Expo lint
- Component tests: React Native Testing Library
- Integration tests: mocked query/auth flows
- Web smoke tests: login, dashboard, exercise browse

## Release Checks

1. Validate that `EXPO_PUBLIC_API_BASE_URL` is set correctly per environment.
2. Keep secure tokens out of committed files.
3. Confirm route redirects behave correctly for anonymous and authenticated sessions.
4. Confirm persisted query cache does not leak user data across logout/login transitions.

## Suggested Next Automation

- Add CI jobs for:
  - `npm ci`
  - `npm run typecheck`
  - `npm run lint`
- Add an Expo/EAS release document once bundle IDs, app icons, and store metadata are finalized.

## Acceptance Criteria

- The frontend has an explicit pre-merge quality gate.
- Release configuration can be added without restructuring the app.

## Handoff Prompt

Start feature delivery on top of the current shell and keep every new screen inside the established route, provider, and token structure.
