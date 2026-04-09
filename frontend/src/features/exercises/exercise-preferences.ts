import { appStorage } from '@/lib/storage';

const MAX_RECENTS = 12;

type ExercisePreferences = {
  favouriteExerciseIds: string[];
  recentExerciseIds: string[];
};

function getStorageKey(userId: string) {
  return `exercise.preferences.${userId}`;
}

async function readPreferences(userId: string): Promise<ExercisePreferences> {
  const raw = await appStorage.getItem(getStorageKey(userId));

  if (!raw) {
    return {
      favouriteExerciseIds: [],
      recentExerciseIds: [],
    };
  }

  try {
    const parsed = JSON.parse(raw) as ExercisePreferences;
    return {
      favouriteExerciseIds: Array.isArray(parsed.favouriteExerciseIds)
        ? parsed.favouriteExerciseIds
        : [],
      recentExerciseIds: Array.isArray(parsed.recentExerciseIds) ? parsed.recentExerciseIds : [],
    };
  } catch {
    return {
      favouriteExerciseIds: [],
      recentExerciseIds: [],
    };
  }
}

async function writePreferences(userId: string, preferences: ExercisePreferences) {
  await appStorage.setItem(getStorageKey(userId), JSON.stringify(preferences));
}

export async function getExercisePreferences(userId: string) {
  return readPreferences(userId);
}

export async function toggleFavouriteExercise(userId: string, exerciseId: string) {
  const preferences = await readPreferences(userId);
  const favourites = new Set(preferences.favouriteExerciseIds);

  if (favourites.has(exerciseId)) {
    favourites.delete(exerciseId);
  } else {
    favourites.add(exerciseId);
  }

  const nextPreferences = {
    ...preferences,
    favouriteExerciseIds: [...favourites],
  };

  await writePreferences(userId, nextPreferences);
  return nextPreferences;
}

export async function recordRecentExercise(userId: string, exerciseId: string) {
  const preferences = await readPreferences(userId);
  const recents = [exerciseId, ...preferences.recentExerciseIds.filter((id) => id != exerciseId)]
    .slice(0, MAX_RECENTS);

  const nextPreferences = {
    ...preferences,
    recentExerciseIds: recents,
  };

  await writePreferences(userId, nextPreferences);
  return nextPreferences;
}
