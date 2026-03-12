# Phase 1: Foundation

## Goal

Stabilize the project as a maintainable frontend workspace rather than a template app.

## Decisions Already Locked

- Keep Expo managed workflow unless a future dependency forces prebuild.
- Keep `src/app` as the router root.
- Use `@/*` path aliases.
- Keep API base URL configurable through `EXPO_PUBLIC_API_BASE_URL`.

## Files and Folders

- `package.json`
- `app.json`
- `tsconfig.json`
- `src/lib/env.ts`
- `src/providers/app-providers.tsx`

## Implementation Tasks

1. Add `typecheck` and `start:clear` scripts.
2. Replace template app identity with project-specific name, slug, scheme, and dark splash configuration.
3. Keep TypeScript strict mode on.
4. Ensure fonts, splash, providers, and system UI all initialize in the root provider stack.
5. Keep secrets out of source and rely on Expo public env vars only for non-secret client config.

## Acceptance Criteria

- `npm run typecheck` passes.
- `npm run lint` passes.
- The app boots into the auth flow without template demo code.

## Handoff Prompt

Implement Phase 2 by expanding the token system, layout primitives, and accessibility rules into a reusable design system.
