import { useState } from 'react';
import { Pressable, StyleSheet, Text } from 'react-native';

import { tokens } from '@/theme/tokens';

type FilterChipProps = {
  label: string;
  selected?: boolean;
  disabled?: boolean;
  onPress: () => void;
};

export function FilterChip({
  label,
  selected = false,
  disabled = false,
  onPress,
}: FilterChipProps) {
  const [hovered, setHovered] = useState(false);

  return (
    <Pressable
      accessibilityLabel={label}
      accessibilityRole="button"
      disabled={disabled}
      onPress={onPress}
      onHoverIn={() => setHovered(true)}
      onHoverOut={() => setHovered(false)}
      style={({ pressed }) => [
        styles.chip,
        selected ? styles.selected : styles.defaultChip,
        hovered && !selected && styles.hovered,
        pressed && styles.pressed,
        disabled && styles.disabled,
      ]}>
      <Text style={[styles.label, selected && styles.selectedLabel]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  chip: {
    minHeight: 38,
    borderRadius: tokens.radius.pill,
    paddingHorizontal: tokens.spacing.md,
    alignItems: 'center',
    justifyContent: 'center',
    borderWidth: 1,
  },
  defaultChip: {
    borderColor: tokens.colors.borderSoft,
    backgroundColor: tokens.colors.surfaceRaised,
  },
  selected: {
    borderColor: tokens.colors.accent,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  hovered: {
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceStrong,
  },
  pressed: {
    opacity: 0.9,
  },
  disabled: {
    opacity: 0.6,
  },
  label: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 13,
  },
  selectedLabel: {
    color: tokens.colors.text,
  },
});
