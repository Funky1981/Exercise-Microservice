import { StyleSheet, Text, View } from 'react-native';

import { tokens } from '@/theme/tokens';

type SectionHeadingProps = {
  eyebrow: string;
  title: string;
  subtitle?: string;
};

export function SectionHeading({ eyebrow, title, subtitle }: SectionHeadingProps) {
  return (
    <View style={styles.wrapper}>
      <Text style={styles.eyebrow}>{eyebrow}</Text>
      <Text style={styles.title}>{title}</Text>
      {subtitle ? <Text style={styles.subtitle}>{subtitle}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: {
    gap: tokens.spacing.xs,
  },
  eyebrow: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    letterSpacing: 1.1,
    textTransform: 'uppercase',
  },
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.display,
    fontSize: 32,
    lineHeight: 38,
  },
  subtitle: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 16,
    lineHeight: 24,
  },
});
