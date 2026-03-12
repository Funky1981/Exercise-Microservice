import { StyleSheet, Text, View } from 'react-native';

import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { env } from '@/lib/env';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export function ProfileScreen() {
  const { session, signOut } = useSession();

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Session"
        title="Profile"
        subtitle="Secure session storage is kept separate from TanStack Query cache so account transitions can clear server state without losing general app shell preferences."
      />

      <GlowCard>
        <View style={styles.row}>
          <Text style={styles.label}>Name</Text>
          <Text style={styles.value}>{session?.name ?? 'Unknown'}</Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.label}>Email</Text>
          <Text style={styles.value}>{session?.email ?? 'Unknown'}</Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.label}>API base URL</Text>
          <Text style={styles.value}>{env.apiBaseUrl}</Text>
        </View>
      </GlowCard>

      <PrimaryButton label="Sign out" onPress={() => void signOut()} tone="muted" />
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  row: {
    gap: tokens.spacing.xs,
  },
  label: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  value: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 15,
  },
});
