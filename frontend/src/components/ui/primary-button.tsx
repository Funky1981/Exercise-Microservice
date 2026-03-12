import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';

import { tokens } from '@/theme/tokens';

type PrimaryButtonProps = {
  label: string;
  onPress: () => void;
  busy?: boolean;
  tone?: 'accent' | 'muted';
};

export function PrimaryButton({
  label,
  onPress,
  busy = false,
  tone = 'accent',
}: PrimaryButtonProps) {
  return (
    <Pressable
      disabled={busy}
      onPress={onPress}
      style={({ pressed }) => [
        styles.button,
        tone === 'muted' ? styles.muted : styles.accent,
        pressed && styles.pressed,
        busy && styles.disabled,
      ]}>
      {busy ? (
        <ActivityIndicator color={tokens.colors.canvas} />
      ) : (
        <Text style={[styles.label, tone === 'muted' && styles.mutedLabel]}>{label}</Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  button: {
    minHeight: 52,
    borderRadius: tokens.radius.pill,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: tokens.spacing.lg,
  },
  accent: {
    backgroundColor: tokens.colors.accent,
  },
  muted: {
    backgroundColor: tokens.colors.surfaceStrong,
  },
  pressed: {
    opacity: 0.88,
    transform: [{ scale: 0.99 }],
  },
  disabled: {
    opacity: 0.65,
  },
  label: {
    color: tokens.colors.canvas,
    fontFamily: tokens.typography.label,
    fontSize: 15,
  },
  mutedLabel: {
    color: tokens.colors.text,
  },
});
