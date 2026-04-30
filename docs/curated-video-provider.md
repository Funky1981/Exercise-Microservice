# Curated Video Provider

The repo now supports a first-class `CuratedManifest` media provider for exercise-specific video coverage.

## Why it exists

- `Wger` is trustworthy but sparse.
- `RapidApi` in the current integration is GIF-only.
- Generic media search providers are too noisy for strict exercise-demo accuracy.

The curated manifest gives you a controlled middle path:

- keep strict video-only media
- keep authoritative matching
- add coverage exercise-by-exercise without changing the frontend contract

## Runtime flow

1. Sync imports exercises from the configured catalogue providers.
2. Media enrichment checks `Wger` and `CuratedManifest`.
3. Curated matches are auto-accepted because they are treated as authoritative.
4. The frontend only displays authoritative video providers, which now includes `Wger` and `CuratedManifest`.

## Manifest location

Default path: `Exercise.API/CuratedMedia/curated-exercise-videos.json`

Override with configuration key: `CuratedMedia:ManifestPath`

Docker Compose passes this through with `CURATED_MEDIA_MANIFEST_PATH`.

## Entry shape

```json
[
  {
    "name": "Push Up",
    "bodyPart": "chest",
    "targetMuscle": "pectorals",
    "equipment": "body weight",
    "aliases": ["Press Up"],
    "sourceMatches": [
      {
        "provider": "Wger",
        "externalId": "123"
      }
    ],
    "videoUrl": "https://cdn.example.com/exercises/push-up.mp4",
    "thumbnailUrl": "https://cdn.example.com/exercises/push-up.jpg",
    "sourcePageUrl": "https://content.example.com/push-up",
    "mediaKind": "video/mp4",
    "sourceTitle": "Push Up Demo"
  }
]
```

## Matching order

1. Exact `sourceMatches` provider + external id
2. Exact normalized `name + bodyPart + targetMuscle`
3. Alias match with the same body part and target muscle

## Operator workflow

1. Add or update curated entries in the manifest.
2. Run `scripts/Invoke-ExerciseSync.ps1`.
3. Open an exercise detail page and confirm the video source is `CuratedManifest` or `Wger`.

The bootstrap script promotes the operator account using either:

- the `exercise_sqlserver` Docker container when it is running
- the `ConnectionStrings:DefaultConnection` value from `dotnet user-secrets` for local development
