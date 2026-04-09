# Phase 4: Data and Auth

## Goal

Connect the frontend shell to the backend safely, with clear ownership over session state and server state.

## Decisions Already Locked

- Session state is not stored in TanStack Query.
- Server state is stored in TanStack Query.
- Secure token storage uses secure storage on native and localStorage fallback on web.
- Refresh flow is centralized inside the API client.

## Files and Folders

- `src/api/client.ts`
- `src/api/types.ts`
- `src/state/session-context.tsx`
- `src/lib/storage.ts`
- `src/lib/query-client.ts`
- `src/lib/query-focus.ts`

## Implementation Tasks

1. Use backend routes exactly as they exist now:
  - `/api/users/register`
  - `/api/auth/login`
  - `/api/auth/refresh`
2. Attach bearer tokens centrally in the API client.
3. Retry once on `401` by refreshing the token pair.
4. Clear query cache and persisted session on logout.
5. Scope query keys by authenticated identity where user switching matters.

## Acceptance Criteria

- Sign-in works against the backend.
- Register works against the backend and can flow into sign-in automatically.
- Authenticated queries recover once from an expired access token.
- Logout clears stale user data from the cache.

## Handoff Prompt

Implement Phase 5 by expanding the current dashboard, catalogue, workout, plan, log, and analytics modules into full CRUD feature slices.
