import { View, StyleSheet } from 'react-native';
import { Tabs } from 'expo-router';

import { SideNav } from '@/components/ui/side-nav';
import { useBreakpoint } from '@/lib/responsive';
import { tokens } from '@/theme/tokens';

export default function TabsLayout() {
  const { isCompact } = useBreakpoint();

  const tabs = (
    <Tabs
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: tokens.colors.text,
        tabBarInactiveTintColor: tokens.colors.textSoft,
        tabBarStyle: isCompact
          ? {
              backgroundColor: tokens.colors.surface,
              borderTopColor: tokens.colors.borderSoft,
              height: 72,
              paddingTop: 8,
              paddingBottom: 8,
            }
          : { display: 'none' },
        tabBarLabelStyle: {
          fontFamily: tokens.typography.label,
          fontSize: 12,
        },
      }}>
      <Tabs.Screen name="index" options={{ title: 'Dashboard' }} />
      <Tabs.Screen name="exercises" options={{ title: 'Exercises' }} />
      <Tabs.Screen name="workouts" options={{ title: 'Workouts' }} />
      <Tabs.Screen name="analytics" options={{ title: 'Analytics' }} />
      <Tabs.Screen name="profile" options={{ title: 'Profile' }} />
    </Tabs>
  );

  if (isCompact) {
    return tabs;
  }

  return (
    <View style={styles.shell}>
      <SideNav />
      <View style={styles.main}>{tabs}</View>
    </View>
  );
}

const styles = StyleSheet.create({
  shell: {
    flex: 1,
    flexDirection: 'row',
  },
  main: {
    flex: 1,
  },
});
