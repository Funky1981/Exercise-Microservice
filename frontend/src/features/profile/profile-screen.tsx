import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';

import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { formatDateTime } from '@/lib/format';
import { env } from '@/lib/env';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export function ProfileScreen() {
  const { session, signOut } = useSession();
  const { showToast } = useToast();

  async function handleSignOut() {
    await signOut();
    showToast({
      tone: 'success',
      title: 'Signed out',
      message: 'The cached session and query state were cleared.',
    });
  }

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Session"
        title="Profile"
        subtitle="Secure session storage stays separate from TanStack Query cache so account changes can clear server data without breaking the shell or navigation state."
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
          <Text style={styles.label}>Access token expiry</Text>
          <Text style={styles.value}>{formatDateTime(session?.expiresAt)}</Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.label}>Refresh token expiry</Text>
          <Text style={styles.value}>{formatDateTime(session?.refreshTokenExpiry)}</Text>
        </View>
        <View style={styles.row}>
          <Text style={styles.label}>API base URL</Text>
          <Text style={styles.value}>{env.apiBaseUrl}</Text>
        </View>
      </GlowCard>

      <GlowCard>
        <Text style={styles.panelTitle}>Quick actions</Text>
        <View style={styles.actions}>
          <PrimaryButton
            label="Workout plans"
            onPress={() => router.push('/(app)/plans' as Href)}
            tone="muted"
            style={styles.actionButton}
          />
          <PrimaryButton
            label="Exercise logs"
            onPress={() => router.push('/(app)/logs' as Href)}
            tone="muted"
            style={styles.actionButton}
          />
        </View>
      </GlowCard>

      <PrimaryButton label="Sign out" onPress={() => void handleSignOut()} tone="muted" />
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
  panelTitle: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 20,
  },
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  actionButton: {
    flexGrow: 1,
    minWidth: 160,
  },
});
