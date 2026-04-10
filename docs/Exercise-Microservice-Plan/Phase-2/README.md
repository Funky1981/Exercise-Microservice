# Phase 2 - Backend Persistence

## Goal
Persist workouts, sets, reps, and timings.

## Changes
- Add WorkoutExercise entity (join table with payload)
- Add RestTimeSeconds
- Add Order

## API
- POST /sessions/start
- POST /sessions/log-set
- POST /sessions/end
