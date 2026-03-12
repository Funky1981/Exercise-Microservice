import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { PrimaryButton } from '@/components/ui/primary-button';
import { TextField } from '@/components/ui/text-field';
import { AuthShell } from '@/features/auth/auth-shell';
import { useToast } from '@/providers/toast-provider';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export default function RegisterScreen() {
  const { register } = useSession();
  const { showToast } = useToast();
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit() {
    try {
      setBusy(true);
      setError(null);
      await register({ name, email, password });
      showToast({
        tone: 'success',
        title: 'Account created',
        message: 'You are signed in and ready to start planning sessions.',
      });
    } catch (nextError) {
      setError(nextError instanceof Error ? nextError.message : 'Unable to register.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <AuthShell
      title="Create account"
      subtitle="The register flow uses the existing backend contract and automatically signs the user in after a successful account creation."
      alternateHref="/(auth)/sign-in"
      alternateLabel="Already registered? Sign in">
      <View style={styles.form}>
        <TextField
          autoCapitalize="words"
          label="Full name"
          placeholder="Alex Morgan"
          value={name}
          onChangeText={setName}
        />
        <TextField
          autoCapitalize="none"
          autoComplete="email"
          keyboardType="email-address"
          label="Email"
          placeholder="alex@example.com"
          value={email}
          onChangeText={setEmail}
        />
        <TextField
          autoCapitalize="none"
          autoComplete="new-password"
          helperText="Backend rules currently expect a strong password."
          label="Password"
          placeholder="Choose a password"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />
        {error ? <Text style={styles.error}>{error}</Text> : null}
        <PrimaryButton busy={busy} label="Register" onPress={() => void handleSubmit()} />
      </View>
    </AuthShell>
  );
}

const styles = StyleSheet.create({
  form: {
    gap: tokens.spacing.md,
  },
  error: {
    color: tokens.colors.danger,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 14,
  },
});
