import {
  ActivityIndicator,
  Pressable,
  StyleSheet,
  Text,
  type StyleProp,
  type TextStyle,
  type ViewStyle,
} from 'react-native';

import { tokens } from '@/theme/tokens';

type PrimaryButtonProps = {
  label: string;
  onPress: () => void;
  busy?: boolean;
  disabled?: boolean;
  tone?: 'accent' | 'muted' | 'danger';
  style?: StyleProp<ViewStyle>;
  labelStyle?: StyleProp<TextStyle>;
};

export function PrimaryButton({
  label,
  onPress,
  busy = false,
  disabled = false,
  tone = 'accent',
  style,
  labelStyle,
}: PrimaryButtonProps) {
  const isDisabled = busy || disabled;

  return (
    <Pressable
      disabled={isDisabled}
      onPress={onPress}
      style={({ pressed }) => [
        styles.button,
        tone === 'muted' ? styles.muted : tone === 'danger' ? styles.danger : styles.accent,
        pressed && styles.pressed,
        isDisabled && styles.disabled,
        style,
      ]}>
      {busy ? (
        <ActivityIndicator color={tokens.colors.canvas} />
      ) : (
        <Text
          style={[
            styles.label,
            (tone === 'muted' || tone === 'danger') && styles.mutedLabel,
            labelStyle,
          ]}>
          {label}
        </Text>
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
  danger: {
    backgroundColor: tokens.colors.danger,
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
