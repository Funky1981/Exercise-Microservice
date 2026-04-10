# Phase 1 - Timer MVP

## Goal
Implement basic workout timers (exercise + rest) in frontend only.

## Features
- Start/stop workout session
- Exercise timer (counts up)
- Rest timer (countdown)
- Set/rep tracking per exercise
- Persist session locally (AsyncStorage)

## Constraints
- No backend changes yet
- Must survive app backgrounding

## Tech Notes
- Use timestamps (Date.now) NOT setInterval
- Use AppState to recalc elapsed time
