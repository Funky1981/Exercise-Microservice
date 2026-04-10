# Timer Service

## Responsibilities
- Track current exercise time
- Track rest countdown
- Handle pause/resume

## Implementation
- Store startTimestamp
- elapsed = now - startTimestamp
- No reliance on JS timers

## Edge Cases
- App backgrounded
- App killed -> recover from storage
