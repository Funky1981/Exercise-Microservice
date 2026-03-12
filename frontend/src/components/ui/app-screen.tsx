import type { PropsWithChildren } from 'react';
import { ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

type AppScreenProps = PropsWithChildren<{
  scrollable?: boolean;
}>;

export function AppScreen({ children, scrollable = true }: AppScreenProps) {
  const { breakpoint } = useBreakpoint();
  const horizontalPadding =
    breakpoint === 'compact' ? tokens.spacing.md : tokens.spacing.xl;

  const content = (
    <View
      style={[
        styles.inner,
        {
          paddingHorizontal: horizontalPadding,
          paddingVertical: tokens.spacing.xl,
        },
      ]}>
      <View style={styles.content}>{children}</View>
    </View>
  );

  return (
    <View style={styles.canvas}>
      <View style={[styles.glow, styles.glowLeft]} />
      <View style={[styles.glow, styles.glowRight]} />
      <SafeAreaView style={styles.safeArea} edges={['top', 'left', 'right']}>
        {scrollable ? (
          <ScrollView contentContainerStyle={styles.scrollContent}>{content}</ScrollView>
        ) : (
          content
        )}
      </SafeAreaView>
    </View>
  );
}

const styles = StyleSheet.create({
  canvas: {
    flex: 1,
    backgroundColor: tokens.colors.canvas,
  },
  safeArea: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
  },
  inner: {
    flexGrow: 1,
    alignItems: 'center',
  },
  content: {
    width: '100%',
    maxWidth: tokens.layout.contentMaxWidth,
    gap: tokens.spacing.lg,
  },
  glow: {
    position: 'absolute',
    width: 280,
    height: 280,
    borderRadius: 280,
  },
  glowLeft: {
    top: -80,
    left: -40,
    backgroundColor: tokens.colors.glowA,
  },
  glowRight: {
    top: 140,
    right: -40,
    backgroundColor: tokens.colors.glowB,
  },
});
