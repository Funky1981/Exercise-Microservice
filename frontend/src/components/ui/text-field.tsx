import { StyleSheet, Text, TextInput, View, type TextInputProps } from 'react-native';

import { tokens } from '@/theme/tokens';

type TextFieldProps = TextInputProps & {
  label: string;
  helperText?: string;
};

export function TextField({ label, helperText, ...props }: TextFieldProps) {
  return (
    <View style={styles.wrapper}>
      <Text style={styles.label}>{label}</Text>
      <TextInput
        placeholderTextColor={tokens.colors.textSoft}
        style={styles.input}
        {...props}
      />
      {helperText ? <Text style={styles.helper}>{helperText}</Text> : null}
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
  input: {
    minHeight: 52,
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    borderColor: tokens.colors.border,
    backgroundColor: tokens.colors.surfaceRaised,
    color: tokens.colors.text,
    paddingHorizontal: tokens.spacing.md,
    paddingVertical: tokens.spacing.sm,
    fontFamily: tokens.typography.body,
    fontSize: 16,
  },
  helper: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.body,
    fontSize: 13,
  },
});
