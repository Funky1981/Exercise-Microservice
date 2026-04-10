import { Pressable, StyleSheet, Text, View } from 'react-native';
import { router, usePathname, type Href } from 'expo-router';

import { tokens } from '@/theme/tokens';

type NavItem = {
  label: string;
  href: Href;
  matchPrefix: string;
};

const navItems: NavItem[] = [
  { label: 'Dashboard', href: '/(app)/(tabs)' as Href, matchPrefix: '/' },
  { label: 'Exercises', href: '/(app)/(tabs)/exercises' as Href, matchPrefix: '/exercises' },
  { label: 'Workouts', href: '/(app)/(tabs)/workouts' as Href, matchPrefix: '/workouts' },
  { label: 'Analytics', href: '/(app)/(tabs)/analytics' as Href, matchPrefix: '/analytics' },
  { label: 'Profile', href: '/(app)/(tabs)/profile' as Href, matchPrefix: '/profile' },
];

export function SideNav() {
  const pathname = usePathname();

  function isActive(item: NavItem) {
    if (item.matchPrefix === '/') {
      return pathname === '/' || pathname === '';
    }
    return pathname.startsWith(item.matchPrefix);
  }

  return (
    <View style={styles.rail}>
      <Text style={styles.brand}>Exercise</Text>
      <View style={styles.navList}>
        {navItems.map((item) => {
          const active = isActive(item);
          return (
            <Pressable
              key={item.label}
              onPress={() => router.push(item.href)}
              style={[styles.navItem, active && styles.navItemActive]}
              accessibilityRole="link"
              accessibilityLabel={item.label}>
              <Text style={[styles.navLabel, active && styles.navLabelActive]}>
                {item.label}
              </Text>
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  rail: {
    width: 220,
    backgroundColor: tokens.colors.surface,
    borderRightWidth: 1,
    borderRightColor: tokens.colors.borderSoft,
    paddingTop: tokens.spacing.xl,
    paddingHorizontal: tokens.spacing.md,
    gap: tokens.spacing.xl,
  },
  brand: {
    color: tokens.colors.accent,
    fontFamily: tokens.typography.display,
    fontSize: 22,
    paddingHorizontal: tokens.spacing.sm,
  },
  navList: {
    gap: tokens.spacing.xs,
  },
  navItem: {
    paddingVertical: tokens.spacing.sm,
    paddingHorizontal: tokens.spacing.sm,
    borderRadius: tokens.radius.sm,
  },
  navItemActive: {
    backgroundColor: tokens.colors.surfaceStrong,
  },
  navLabel: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 15,
  },
  navLabelActive: {
    color: tokens.colors.text,
  },
});
