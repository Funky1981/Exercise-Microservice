import { useState } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { AuthShell } from '@/features/auth/auth-shell';
import { PrimaryButton } from '@/components/ui/primary-button';
import { TextField } from '@/components/ui/text-field';
import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export default function SignInScreen() {
  const { signIn } = useSession();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit() {
    try {
      setBusy(true);
      setError(null);
      await signIn({ email, password });
    } catch (nextError) {
      setError(nextError instanceof Error ? nextError.message : 'Unable to sign in.');
    } finally {
      setBusy(false);
    }
  }

  return (
    <AuthShell
      title="Sign in"
      subtitle="A dark, responsive entry point for the app shell and backend-authenticated flows."
      alternateHref="/(auth)/register"
      alternateLabel="Need an account? Register">
      <View style={styles.form}>
        <TextField
          autoCapitalize="none"
          autoComplete="email"
          keyboardType="email-address"
          label="Email"
          placeholder="you@example.com"
          value={email}
          onChangeText={setEmail}
        />
        <TextField
          autoCapitalize="none"
          autoComplete="password"
          label="Password"
          placeholder="Enter your password"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />
        {error ? <Text style={styles.error}>{error}</Text> : null}
        <PrimaryButton busy={busy} label="Sign in" onPress={() => void handleSubmit()} />
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
