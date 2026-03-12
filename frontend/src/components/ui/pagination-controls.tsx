import { startTransition } from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { PrimaryButton } from '@/components/ui/primary-button';
import { tokens } from '@/theme/tokens';

type PaginationControlsProps = {
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  busy?: boolean;
  onPageChange: (pageNumber: number) => void;
};

export function PaginationControls({
  pageNumber,
  totalPages,
  totalCount,
  busy = false,
  onPageChange,
}: PaginationControlsProps) {
  const safeTotalPages = Math.max(totalPages, 1);

  return (
    <View style={styles.wrapper}>
      <View style={styles.summary}>
        <Text style={styles.summaryLabel}>Page</Text>
        <Text style={styles.summaryValue}>
          {pageNumber} of {safeTotalPages}
        </Text>
        <Text style={styles.summaryMeta}>{totalCount} total items</Text>
      </View>
      <View style={styles.actions}>
        <PrimaryButton
          label="Previous"
          onPress={() => startTransition(() => onPageChange(Math.max(pageNumber - 1, 1)))}
          tone="muted"
          disabled={busy || pageNumber <= 1}
          style={styles.action}
        />
        <PrimaryButton
          label="Next"
          onPress={() =>
            startTransition(() =>
              onPageChange(Math.min(pageNumber + 1, safeTotalPages))
            )
          }
          disabled={busy || pageNumber >= safeTotalPages}
          style={styles.action}
        />
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  wrapper: {
    gap: tokens.spacing.sm,
  },
  summary: {
    gap: tokens.spacing.xs,
  },
  summaryLabel: {
    color: tokens.colors.textSoft,
    fontFamily: tokens.typography.label,
    fontSize: 12,
    textTransform: 'uppercase',
    letterSpacing: 0.8,
  },
  summaryValue: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.heading,
    fontSize: 18,
  },
  summaryMeta: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
  },
  actions: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: tokens.spacing.sm,
  },
  action: {
    minWidth: 140,
    flexGrow: 1,
  },
});
