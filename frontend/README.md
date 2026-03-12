# Exercise Frontend

Expo-managed React Native frontend for the Exercise Microservice.

## Stack

- Expo SDK 55
- Expo Router
- React Native 0.83
- React 19
- TanStack Query v5
- Secure session persistence for native, localStorage fallback for web

## Current State

This branch includes:

- a dark-first responsive app shell
- auth and tab route groups
- shared provider stack
- backend-aware auth/session wiring
- query persistence and AppState focus handling
- foundation screens for dashboard, exercises, workouts, and profile
- phased implementation docs in `docs/`

## Run

```bash
npm install
npm run start
```

Useful commands:

```bash
npm run typecheck
npm run lint
npx expo export --platform web
```

## VS Code Debug

Use the workspace debug configs in `.vscode/launch.json`:

- `Full Stack: API + Expo Web` starts the backend and opens the Expo web app
- `Full Stack: API + Expo Dev Server` starts the backend and the Expo dev server for device/emulator use
- `Exercise API` starts only the backend
- `Exercise API (coreclr)` is the browser-opening backend launch for the classic C# debugger
- `Exercise API (dotnet)` is available if you have the newer `.NET` debug extension/C# Dev Kit support
- `Expo Web` or `Expo Dev Server` start only the frontend

The compound launch targets use the classic `coreclr` C# debugger for compatibility. The optional `dotnet` launch target is only for VS Code setups that support the newer `.NET` debugger type.

The frontend debug configs inject:

```bash
EXPO_PUBLIC_API_BASE_URL=http://localhost:5034
```

The VS Code Expo debug targets also force port `8082` because `8081` is commonly occupied by Docker Desktop/WSL on this machine.

## Environment

Set the backend base URL with:

```bash
EXPO_PUBLIC_API_BASE_URL=http://localhost:5034
```

If the variable is omitted, the app falls back to:

- Android emulator: `http://10.0.2.2:5034`
- iOS, web, desktop dev: `http://localhost:5034`

## Docs

Implementation phases live in:

- `docs/00-overview.md`
- `docs/01-foundation.md`
- `docs/02-design-system.md`
- `docs/03-app-shell-navigation.md`
- `docs/04-data-auth.md`
- `docs/05-core-features.md`
- `docs/06-responsive-web.md`
- `docs/07-testing-release.md`
