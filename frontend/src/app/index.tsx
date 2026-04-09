import { Redirect } from 'expo-router';
import { ActivityIndicator, View } from 'react-native';

import { useSession } from '@/state/session-context';
import { tokens } from '@/theme/tokens';

export default function IndexRoute() {
  const { status } = useSession();

  if (status === 'booting') {
    return (
      <View
        style={{
          flex: 1,
          backgroundColor: tokens.colors.canvas,
          alignItems: 'center',
          justifyContent: 'center',
        }}>
        <ActivityIndicator color={tokens.colors.accent} size="large" />
      </View>
    );
  }

  if (status === 'authenticated') {
    return <Redirect href="/(app)/(tabs)" />;
  }

  return <Redirect href="/(auth)/sign-in" />;
}
