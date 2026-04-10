import { useEffect, useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { router, type Href } from 'expo-router';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import { apiClient } from '@/api/client';
import { queryKeys } from '@/api/query-keys';
import { AppScreen } from '@/components/ui/app-screen';
import { GlowCard } from '@/components/ui/glow-card';
import { PrimaryButton } from '@/components/ui/primary-button';
import { SectionHeading } from '@/components/ui/section-heading';
import { TextField } from '@/components/ui/text-field';
import { formatDateTime } from '@/lib/format';
import { useBreakpoint } from '@/lib/responsive';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export function ProfileScreen() {
  const { session, signOut } = useSession();
  const { showToast } = useToast();
  const { isCompact } = useBreakpoint();
  const queryClient = useQueryClient();

  const profileQuery = useQuery({
    queryKey: queryKeys.users.profile(session?.userId),
    queryFn: () => apiClient.getUserProfile(session!.userId),
    enabled: Boolean(session?.userId),
  });

  const [userName, setUserName] = useState('');
  const [heightCm, setHeightCm] = useState('');
  const [weightKg, setWeightKg] = useState('');
  const [isEditing, setIsEditing] = useState(false);

  // Seed form fields when profile loads
  useEffect(() => {
    if (profileQuery.data) {
      setUserName(profileQuery.data.userName ?? '');
      setHeightCm(profileQuery.data.heightCm?.toString() ?? '');
      setWeightKg(profileQuery.data.weightKg?.toString() ?? '');
    }
  }, [profileQuery.data]);

  const updateMutation = useMutation({
    mutationFn: () =>
      apiClient.updateUserProfile(session!.userId, {
        userName: userName.trim() || null,
        heightCm: heightCm ? Number(heightCm) : null,
        weightKg: weightKg ? Number(weightKg) : null,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: queryKeys.users.profile(session?.userId),
      });
      setIsEditing(false);
      showToast({ tone: 'success', title: 'Profile updated' });
    },
    onError: (err) => {
      showToast({
        tone: 'error',
        title: 'Update failed',
        message: err instanceof Error ? err.message : 'Try again.',
      });
    },
  });

  async function handleSignOut() {
    await signOut();
    showToast({
      tone: 'success',
      title: 'Signed out',
      message: 'The cached session and query state were cleared.',
    });
  }

  const profile = profileQuery.data;

  return (
    <AppScreen>
      <SectionHeading
        eyebrow="Account"
        title="Profile"
        subtitle={profile ? `Member since ${formatDateTime(profile.createdAt)}` : undefined}
      />

      <GlowCard>
        <View style={[styles.infoGrid, !isCompact && styles.infoGridWide]}>
          <View style={styles.infoColumn}>
            <View style={styles.row}>
              <Text style={styles.label}>Name</Text>
              <Text style={styles.value}>{session?.name ?? 'Unknown'}</Text>
            </View>
            <View style={styles.row}>
              <Text style={styles.label}>Email</Text>
              <Text style={styles.value}>{session?.email ?? 'Unknown'}</Text>
            </View>
          </View>

          <View style={styles.infoColumn}>
            {isEditing ? (
              <>
                <TextField
                  label="Username"
                  value={userName}
                  onChangeText={setUserName}
                  helperText="Optional display name"
                />
                <TextField
                  label="Height (cm)"
                  value={heightCm}
                  onChangeText={setHeightCm}
                  keyboardType="decimal-pad"
                />
                <TextField
                  label="Weight (kg)"
                  value={weightKg}
                  onChangeText={setWeightKg}
                  keyboardType="decimal-pad"
                />
                <View style={styles.editActions}>
                  <PrimaryButton
                    label="Save"
                    onPress={() => updateMutation.mutate()}
                    busy={updateMutation.isPending}
                    style={styles.editBtn}
                  />
                  <PrimaryButton
                    label="Cancel"
                    onPress={() => setIsEditing(false)}
                    tone="muted"
                    style={styles.editBtn}
                  />
                </View>
              </>
            ) : (
              <>
                <View style={styles.row}>
                  <Text style={styles.label}>Username</Text>
                  <Text style={styles.value}>{profile?.userName ?? 'Not set'}</Text>
                </View>
                <View style={styles.row}>
                  <Text style={styles.label}>Height</Text>
                  <Text style={styles.value}>
                    {profile?.heightCm ? `${profile.heightCm} cm` : 'Not set'}
                  </Text>
                </View>
                <View style={styles.row}>
                  <Text style={styles.label}>Weight</Text>
                  <Text style={styles.value}>
                    {profile?.weightKg ? `${profile.weightKg} kg` : 'Not set'}
                  </Text>
                </View>
                <PrimaryButton
                  label="Edit profile"
                  onPress={() => setIsEditing(true)}
                  tone="muted"
                />
              </>
            )}
          </View>
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

      <PrimaryButton label="Sign out" onPress={() => void handleSignOut()} tone="danger" />
    </AppScreen>
  );
}

const styles = StyleSheet.create({
  infoGrid: {
    gap: tokens.spacing.md,
  },
  infoGridWide: {
    flexDirection: 'row',
  },
  infoColumn: {
    flex: 1,
    gap: tokens.spacing.md,
  },
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
  editActions: {
    flexDirection: 'row',
    gap: tokens.spacing.sm,
  },
  editBtn: {
    flex: 1,
  },
});
