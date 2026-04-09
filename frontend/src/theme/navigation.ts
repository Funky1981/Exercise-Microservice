import { DarkTheme, type Theme } from '@react-navigation/native';

import { tokens } from '@/theme/tokens';

export const navigationTheme: Theme = {
  ...DarkTheme,
  colors: {
    ...DarkTheme.colors,
    primary: tokens.colors.accent,
    background: tokens.colors.canvas,
    card: tokens.colors.surface,
    text: tokens.colors.text,
    border: tokens.colors.borderSoft,
    notification: tokens.colors.accentWarm,
  },
};
