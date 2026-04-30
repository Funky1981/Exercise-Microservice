type ExerciseMediaFields = {
  mediaUrl?: string | null;
  mediaKind?: string | null;
  mediaThumbnailUrl?: string | null;
  mediaSourceProvider?: string | null;
};

const VIDEO_EXTENSIONS = ['.mp4', '.webm', '.mov', '.m4v', '.avi', '.ogg'];
const AUTHORITATIVE_PROVIDERS = new Set(['wger', 'curatedmanifest']);

export function isPlayableExerciseMedia(mediaKind?: string | null, mediaUrl?: string | null) {
  if (mediaKind?.toLowerCase().startsWith('video/')) {
    return true;
  }

  if (!mediaUrl) {
    return false;
  }

  const lower = mediaUrl.toLowerCase();
  return VIDEO_EXTENSIONS.some((extension) => lower.endsWith(extension));
}

export function isVideoExerciseMedia(mediaKind?: string | null, mediaUrl?: string | null) {
  if (mediaKind?.toLowerCase().startsWith('video/')) {
    return true;
  }

  if (!mediaUrl) {
    return false;
  }

  const lower = mediaUrl.toLowerCase();
  return VIDEO_EXTENSIONS.some((extension) => lower.endsWith(extension));
}

function isVerifiedVideoExerciseMedia(exercise: ExerciseMediaFields) {
  return isVideoExerciseMedia(exercise.mediaKind, exercise.mediaUrl)
    && Boolean(exercise.mediaSourceProvider)
    && AUTHORITATIVE_PROVIDERS.has(exercise.mediaSourceProvider.toLowerCase());
}

export function getPlayableExerciseMediaUrl(exercise: ExerciseMediaFields) {
  if (isVerifiedVideoExerciseMedia(exercise)) {
    return exercise.mediaUrl ?? null;
  }

  return null;
}

export function getPlayableExercisePreviewUrl(exercise: ExerciseMediaFields) {
  if (isVerifiedVideoExerciseMedia(exercise)) {
    return exercise.mediaThumbnailUrl ?? null;
  }

  return null;
}

export function hasPlayableExerciseMedia(exercise: ExerciseMediaFields) {
  return Boolean(getPlayableExerciseMediaUrl(exercise));
}