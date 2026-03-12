import type { PropsWithChildren } from 'react';
import { Pressable, StyleSheet, Text, View } from 'react-native';
import { Link } from 'expo-router';

import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { SectionHeading } from '@/components/ui/section-heading';
import { useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

type AuthShellProps = PropsWithChildren<{
  title: string;
  subtitle: string;
  alternateHref: '/(auth)/sign-in' | '/(auth)/register';
  alternateLabel: string;
}>;

export function AuthShell({
  title,
  subtitle,
  alternateHref,
  alternateLabel,
  children,
}: AuthShellProps) {
  const { isExpanded } = useBreakpoint();

  return (
    <AppScreen scrollable={false}>
      <View style={[styles.layout, isExpanded && styles.layoutExpanded]}>
        <View style={styles.hero}>
          <SectionHeading
            eyebrow="Exercise Frontend"
            title={title}
            subtitle={subtitle}
          />
          <Text style={styles.copy}>
            Built on Expo Router, responsive layout primitives, and TanStack Query-backed
            server state.
          </Text>
        </View>

        <GlowCard style={styles.panel}>
          {children}
          <Link href={alternateHref} asChild>
            <Pressable>
              <Text style={styles.linkLabel}>{alternateLabel}</Text>
            </Pressable>
          </Link>
        </GlowCard>
      </View>
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  layout: {
    flex: 1,
    justifyContent: 'center',
    gap: tokens.spacing.xl,
  },
  layoutExpanded: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  hero: {
    flex: 1,
    gap: tokens.spacing.md,
  },
  copy: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 16,
    lineHeight: 24,
    maxWidth: 560,
  },
  panel: {
    flex: 1,
    maxWidth: 520,
  },
  linkLabel: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.label,
    fontSize: 14,
  },
});
