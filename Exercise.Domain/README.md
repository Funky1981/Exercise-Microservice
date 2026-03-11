# Exercise.Domain

`Exercise.Domain` contains the core business model. It has no project dependencies and does not know about EF Core, HTTP, or infrastructure concerns.

## Entities

- `User`
- `Exercise`
- `Workout`
- `WorkoutPlan`
- `ExerciseLog`
- `Analytics`

## Value Objects

- `Height`
- `Weight`

## Shared Domain Utilities

- `Common/Guard.cs`

## Domain Rules

### User

- Requires a valid identity and email.
- Supports password-based auth and provider-based auth.
- Refresh token hashes and expiries are stored on the aggregate.
- Profile updates happen through domain methods, not public setters.

### Exercise

- Requires `Name`, `BodyPart`, and `TargetMuscle`.
- Supports optional descriptive metadata such as equipment, GIF URL, description, and difficulty.

### Workout

- Belongs to a single user.
- Prevents duplicate exercises.
- Cannot be modified after completion.
- Completion requires a positive duration.

### WorkoutPlan

- Belongs to a single user.
- Can aggregate multiple workouts.
- Validates date ranges.
- Controls active/inactive state through behavior methods.

### ExerciseLog

- Belongs to a single user.
- Tracks completed exercise entries.
- Cannot be modified after completion.
- Can calculate total duration from its entries.

### Analytics

- Represents generated user analytics for a time window.
- Stores flexible metric data in a dictionary-backed payload.

## Encapsulation

The domain favors:

- Private setters
- Constructor and method guards
- Backing collections exposed as `IReadOnlyList`
- Behavior methods that enforce invariants

## Persistence Considerations

The domain is persistence-agnostic, but the current infrastructure maps several concepts onto it:

- Soft delete
- Audit timestamps
- Optimistic concurrency tokens

Those are implemented as infrastructure concerns rather than domain properties.

## Build

```bash
dotnet build Exercise.Domain/Exercise.Domain.csproj
```
