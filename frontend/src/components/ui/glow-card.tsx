import type { PropsWithChildren } from 'react';
import { Platform, StyleSheet, View, type StyleProp, type ViewStyle } from 'react-native';

import { tokens } from '@/theme/tokens';

type GlowCardProps = PropsWithChildren<{
  style?: StyleProp<ViewStyle>;
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
    ...(Platform.OS === 'web'
      ? {
          boxShadow: '0px 12px 18px rgba(0, 0, 0, 0.18)',
        }
      : {
          shadowColor: '#000000',
          shadowOpacity: 0.18,
          shadowRadius: 18,
          shadowOffset: { width: 0, height: 12 },
          elevation: 8,
        }),
  },
});
