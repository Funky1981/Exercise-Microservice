import { useMemo, useState } from 'react';
import DateTimePicker from '@react-native-community/datetimepicker';
import { Platform, Pressable, StyleSheet, Text, View } from 'react-native';

import { tokens } from '@/theme/tokens';

type DateTimeFieldProps = {
  label: string;
  mode: 'date' | 'time';
  value: string;
  onChange: (value: string) => void;
  helperText?: string;
  disabled?: boolean;
};

export function DateTimeField({
  label,
  mode,
  value,
  onChange,
  helperText,
  disabled = false,
}: DateTimeFieldProps) {
  const [pickerVisible, setPickerVisible] = useState(false);
  const pickerDate = useMemo(() => toPickerDate(mode, value), [mode, value]);

  return (
    <View style={styles.wrapper}>
      <Text style={styles.label}>{label}</Text>
      {Platform.OS === 'web' ? (
        <input
          aria-label={label}
          disabled={disabled}
          onChange={(event) => onChange(event.currentTarget.value)}
          style={webInputStyle}
          type={mode}
          value={value}
        />
      ) : (
        <>
          <Pressable
            accessibilityLabel={label}
            accessibilityRole="button"
            disabled={disabled}
            onPress={() => setPickerVisible(true)}
            style={({ pressed }) => [
              styles.inputButton,
              pressed && styles.pressed,
              disabled && styles.disabled,
            ]}>
            <Text style={[styles.value, !value && styles.placeholder]}>
              {value || (mode === 'date' ? 'Choose a date' : 'Choose a time')}
            </Text>
          </Pressable>
          {pickerVisible ? (
            <DateTimePicker
              display="default"
              mode={mode}
              onChange={(_, selectedDate) => {
                setPickerVisible(false);
                if (!selectedDate) {
                  return;
                }

                onChange(formatPickerValue(mode, selectedDate));
              }}
              value={pickerDate}
            />
          ) : null}
        </>
      )}
      {helperText ? <Text style={styles.helper}>{helperText}</Text> : null}
    </View>
  );
}

function toPickerDate(mode: 'date' | 'time', value: string) {
  if (!value) {
    return new Date();
  }

  if (mode === 'date') {
    return new Date(`${value}T12:00:00`);
  }

  return new Date(`1970-01-01T${value}:00`);
}

function formatPickerValue(mode: 'date' | 'time', value: Date) {
  if (mode === 'date') {
    return value.toISOString().slice(0, 10);
  }

  return value.toISOString().slice(11, 16);
}

const webInputStyle: React.CSSProperties = {
  minHeight: 52,
  borderRadius: tokens.radius.md,
  borderWidth: 1,
  borderStyle: 'solid',
  borderColor: tokens.colors.border,
  backgroundColor: tokens.colors.surfaceRaised,
  color: tokens.colors.text,
  padding: `0 ${tokens.spacing.md}px`,
  fontFamily: tokens.typography.body,
  fontSize: 16,
  outline: 'none',
};

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
  inputButton: {
    minHeight: 52,
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceRaised,
    justifyContent: 'center',
    paddingHorizontal: tokens.spacing.md,
  },
  value: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.body,
    fontSize: 16,
  },
  placeholder: {
    color: tokens.colors.textSoft,
  },
  helper: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 13,
  },
  pressed: {
    opacity: 0.9,
  },
  disabled: {
    opacity: 0.65,
  },
});
