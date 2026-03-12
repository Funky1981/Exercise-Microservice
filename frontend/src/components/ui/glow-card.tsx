import type { PropsWithChildren } from 'react';
import { StyleSheet, View, type ViewStyle } from 'react-native';

import { tokens } from '@/theme/tokens';

type GlowCardProps = PropsWithChildren<{
  style?: ViewStyle;
}>;

export function GlowCard({ children, style }: GlowCardProps) {
  return <View style={[styles.card, style]}>{children}</View>;
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: tokens.colors.surface,
    borderRadius: tokens.radius.lg,
    borderWidth: 1,
    borderColor: tokens.colors.borderSoft,
    padding: tokens.spacing.lg,
    gap: tokens.spacing.md,
    shadowColor: '#000000',
    shadowOpacity: 0.18,
    shadowRadius: 18,
    shadowOffset: { width: 0, height: 12 },
    elevation: 8,
  },
});
