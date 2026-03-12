# Phase 0: Overview

## Goal

Define the locked frontend direction for a universal Expo-managed React Native app that targets iOS, Android, and web from one codebase.

## Context7 Alignment

- Expo: use Expo Router, TypeScript, root `_layout.tsx`, custom font loading, and splash-screen coordination.
- React Native: prefer `useWindowDimensions`, safe-area handling, flexible layouts, and lazy lists instead of fixed-size screens.
- TanStack Query: use a shared `QueryClient`, `PersistQueryClientProvider`, and AppState-backed focus management.

## Locked Decisions

- Stack: Expo SDK 55, Expo Router, React 19, React Native 0.83.
- Theme direction: dark-first only in v1, tokenized so light mode can be added later.
- Server state: TanStack Query v5 with persisted cache.
- Session storage: secure storage on native, localStorage fallback on web.
- Navigation model: route groups for `(auth)` and `(app)/(tabs)`.
- Responsive buckets:
  - `compact`: `<= 599`
  - `medium`: `600-1023`
  - `expanded`: `>= 1024`

## Current Foundation

- `src/app` contains route groups and root layout.
- `src/providers` owns app-wide provider composition.
- `src/state/session-context.tsx` owns session boot, sign-in, sign-out, and persistence.
- `src/api/client.ts` talks to the existing backend contract.
- `src/theme` holds design tokens and navigation theme wiring.
- `src/lib` holds environment, storage, query client, query focus, and responsive helpers.

## Files to Know First

- `src/app/_layout.tsx`
- `src/providers/app-providers.tsx`
- `src/state/session-context.tsx`
- `src/api/client.ts`
- `src/theme/tokens.ts`

## Acceptance Criteria

- A new contributor can identify the app shell, data layer, and route groups in under five minutes.
- The architecture makes it obvious where new feature modules should live.

## Handoff Prompt

Implement Phase 1 by tightening workspace tooling, environment handling, linting, and developer conventions without changing the route architecture.
