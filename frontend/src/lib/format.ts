function formatDurationPart(value: number) {
  return value.toString().padStart(2, '0');
}

export function formatDate(dateValue?: string | null) {
  if (!dateValue) {
    return 'No date';
  }

  return new Date(dateValue).toLocaleDateString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

export function formatDateTime(dateValue?: string | null) {
  if (!dateValue) {
    return 'No date';
  }

  return new Date(dateValue).toLocaleString(undefined, {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  });
}

export function formatDuration(duration?: string | null) {
  if (!duration) {
    return 'Not recorded';
  }

  const [hours = '0', minutes = '0', seconds = '0'] = duration.split(':');
  const parts = [];

  if (Number(hours) > 0) {
    parts.push(`${Number(hours)}h`);
  }

  if (Number(minutes) > 0) {
    parts.push(`${Number(minutes)}m`);
  }

  if (Number(seconds) > 0 || parts.length === 0) {
    parts.push(`${Number(seconds)}s`);
  }

  return parts.join(' ');
}

export function isoDateFromInput(value: string) {
  return new Date(`${value}T09:00:00`).toISOString();
}

export function inputDateFromIso(value?: string | null) {
  if (!value) {
    return '';
  }

  return new Date(value).toISOString().slice(0, 10);
}

export function normalizeOptionalText(value: string) {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : undefined;
}

export function normalizeOptionalNullableText(value: string) {
  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

export function minutesToDuration(minutes: number) {
  const safeMinutes = Math.max(0, Math.floor(minutes));
  const hours = Math.floor(safeMinutes / 60);
  const remainingMinutes = safeMinutes % 60;

  return `${formatDurationPart(hours)}:${formatDurationPart(remainingMinutes)}:00`;
}
