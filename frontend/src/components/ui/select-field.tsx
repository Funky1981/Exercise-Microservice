import { Modal, Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import { useRef, useState } from 'react';

import { tokens } from '@/theme/tokens';

type SelectFieldOption = {
  label: string;
  value: string;
};

type SelectFieldProps = {
  label: string;
  value: string;
  onValueChange: (value: string) => void;
  options: SelectFieldOption[];
  helperText?: string;
  enabled?: boolean;
};

export function SelectField({
  label,
  value,
  onValueChange,
  options,
  helperText,
  enabled = true,
}: SelectFieldProps) {
  const [open, setOpen] = useState(false);
  const [dropdownPos, setDropdownPos] = useState({ top: 0, left: 0, width: 0 });
  const triggerRef = useRef<View>(null);

  const selectedOption = options.find((option) => option.value === value) ?? options[0];

  function handleOpenPress() {
    if (!enabled) return;

    triggerRef.current?.measureInWindow((x, y, width, height) => {
      setDropdownPos({ top: y + height + 4, left: x, width });
      setOpen(true);
    });
  }

  function handleSelect(nextValue: string) {
    onValueChange(nextValue);
    setOpen(false);
  }

  return (
    <View style={styles.wrapper}>
      <Text style={styles.label}>{label}</Text>
      <View ref={triggerRef} collapsable={false}>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel={label}
          disabled={!enabled}
          onPress={handleOpenPress}
          style={({ pressed }) => [
            styles.trigger,
            !enabled && styles.disabled,
            pressed && enabled && styles.triggerPressed,
            open && styles.triggerOpen,
          ]}>
          <Text style={[styles.value, !enabled && styles.disabledValue]}>
            {selectedOption?.label ?? 'Select an option'}
          </Text>
          <Text style={styles.chevron}>{open ? '▲' : '▼'}</Text>
        </Pressable>
      </View>
      {helperText ? <Text style={styles.helper}>{helperText}</Text> : null}

      <Modal
        transparent
        visible={open}
        onRequestClose={() => setOpen(false)}
        animationType="none">
        {/* Full-screen backdrop — tap outside to dismiss */}
        <Pressable style={StyleSheet.absoluteFillObject} onPress={() => setOpen(false)} />
        {/* Dropdown floats at the trigger's window-relative position */}
        <View
          style={[
            styles.dropdown,
            { top: dropdownPos.top, left: dropdownPos.left, width: dropdownPos.width },
          ]}>
          {options.map((option) => {
            const selected = option.value === value;

            return (
              <Pressable
                key={option.value}
                accessibilityRole="button"
                accessibilityLabel={`${label}: ${option.label}`}
                onPress={() => handleSelect(option.value)}
                style={({ pressed }) => [
                  styles.option,
                  selected && styles.optionSelected,
                  pressed && styles.optionPressed,
                ]}>
                <Text style={[styles.optionText, selected && styles.optionTextSelected]}>
                  {option.label}
                </Text>
              </Pressable>
            );
          })}
        </View>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: {
    gap: tokens.spacing.sm,
  },
  label: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.label,
    fontSize: 13,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  trigger: {
    minHeight: 52,
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceRaised,
    paddingHorizontal: tokens.spacing.md,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: tokens.spacing.sm,
  },
  triggerOpen: {
    borderColor: tokens.colors.accent,
  },
  triggerPressed: {
    backgroundColor: tokens.colors.surfaceStrong,
  },
  value: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.body,
    fontSize: 16,
    flex: 1,
  },
  chevron: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
  },
  dropdown: {
    position: 'absolute',
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surface,
    overflow: 'hidden',
    ...(Platform.OS === 'web'
      ? ({ boxShadow: '0px 8px 24px rgba(0, 0, 0, 0.3)' } as object)
      : {
          shadowColor: '#000000',
          shadowOpacity: 0.25,
          shadowRadius: 12,
          shadowOffset: { width: 0, height: 6 },
          elevation: 10,
        }),
  },
  option: {
    paddingHorizontal: tokens.spacing.md,
    paddingVertical: tokens.spacing.md,
    backgroundColor: tokens.colors.surface,
  },
  optionSelected: {
    backgroundColor: tokens.colors.surfaceStrong,
  },
  optionPressed: {
    backgroundColor: tokens.colors.surfaceRaised,
  },
  optionText: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.body,
    fontSize: 15,
  },
  optionTextSelected: {
    fontFamily: tokens.typography.bodyStrong,
  },
  helper: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 13,
  },
  disabled: {
    opacity: 0.65,
  },
  disabledValue: {
    color: tokens.colors.textSoft,
  },
});
