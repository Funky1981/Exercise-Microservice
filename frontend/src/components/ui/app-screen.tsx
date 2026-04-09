import type { PropsWithChildren } from 'react';
import { ScrollView, StyleSheet, View, type StyleProp, type ViewStyle } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import * as ExpoRouter from 'expo-router';

import { PrimaryButton } from '@/components/ui/primary-button';
import { useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

type AppScreenProps = PropsWithChildren<{
  scrollable?: boolean;
  contentStyle?: StyleProp<ViewStyle>;
  maxWidth?: number;
}>;

export function AppScreen({
  children,
  scrollable = true,
  contentStyle,
  maxWidth = tokens.layout.contentMaxWidth,
}: AppScreenProps) {
  const { breakpoint } = useBreakpoint();
  const pathname = ExpoRouter.usePathname?.() ?? '';
  const segments = ExpoRouter.useSegments?.() ?? [];
  const horizontalPadding =
    breakpoint === 'compact'
      ? tokens.spacing.md
      : breakpoint === 'medium'
        ? tokens.spacing.lg
        : tokens.spacing.xl;
  const showSectionNav =
    segments[0] === '(app)' &&
    !segments.some((segment) => segment === '(tabs)') &&
    ['plans', 'logs', 'workouts', 'exercises', 'profile'].includes(segments[1] ?? '');

  const content = (
    <View
      style={[
        styles.inner,
        {
          paddingHorizontal: horizontalPadding,
          paddingTop: tokens.spacing.xl,
          paddingBottom: tokens.spacing.xxl,
        },
      ]}>
      <View style={[styles.content, { maxWidth }, contentStyle]}>
        {showSectionNav ? <SectionNav pathname={pathname} /> : null}
        {children}
      </View>
    </View>
  );

  return (
    <View style={styles.canvas}>
      <View style={[styles.glow, styles.glowLeft]} />
      <View style={[styles.glow, styles.glowRight]} />
      <SafeAreaView style={styles.safeArea} edges={['top', 'right', 'bottom', 'left']}>
        {scrollable ? (
          <ScrollView contentContainerStyle={styles.scrollContent}>{content}</ScrollView>
        ) : (
          content
        )}
      </SafeAreaView>
    </View>
  );
}

function SectionNav({ pathname }: { pathname: string }) {
  const items: Array<{ label: string; href: ExpoRouter.Href; active: boolean }> = [
    { label: 'Dashboard', href: '/(app)/(tabs)', active: pathname === '/' },
    { label: 'Exercises', href: '/(app)/(tabs)/exercises', active: pathname.startsWith('/exercises') },
    { label: 'Workouts', href: '/(app)/(tabs)/workouts', active: pathname.startsWith('/workouts') },
    { label: 'Plans', href: '/(app)/plans', active: pathname.startsWith('/plans') },
    { label: 'Logs', href: '/(app)/logs', active: pathname.startsWith('/logs') },
    { label: 'Profile', href: '/(app)/(tabs)/profile', active: pathname.startsWith('/profile') },
  ];

  return (
    <View style={styles.sectionNav}>
      {items.map((item) => (
        <PrimaryButton
          key={item.label}
          label={item.label}
          onPress={() => ExpoRouter.router.replace(item.href)}
          tone={item.active ? 'accent' : 'muted'}
          style={styles.sectionNavButton}
        />
      ))}
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
    gap: tokens.spacing.lg,
  },
  sectionNav: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  sectionNavButton: {
    minWidth: 120,
    flexGrow: 1,
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
