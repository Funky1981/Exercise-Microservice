import { ActivityIndicator, StyleSheet, Text } from 'react-native';

import { GlowCard } from '@/components/ui/glow-card';
import { tokens } from '@/theme/tokens';

type StatusCardProps = {
  title: string;
  body: string;
  busy?: boolean;
};

export function StatusCard({ title, body, busy = false }: StatusCardProps) {
  return (
    <GlowCard>
      {busy ? <ActivityIndicator color={tokens.colors.accent} /> : null}
      <Text style={styles.title}>{title}</Text>
      <Text style={styles.body}>{body}</Text>
    </GlowCard>
  );
}

const styles = StyleSheet.create({
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  body: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 15,
    lineHeight: 22,
  },
});
