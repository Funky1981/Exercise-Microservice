import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type PropsWithChildren,
} from 'react';
import { Animated, StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { tokens } from '@/theme/tokens';

type ToastTone = 'info' | 'success' | 'error';

type ToastInput = {
  title: string;
  message?: string;
  tone?: ToastTone;
};

type ToastState = ToastInput & {
  id: number;
};

type ToastContextValue = {
  showToast: (toast: ToastInput) => void;
};

const ToastContext = createContext<ToastContextValue | null>(null);

export function ToastProvider({ children }: PropsWithChildren) {
  const insets = useSafeAreaInsets();
  const [toast, setToast] = useState<ToastState | null>(null);
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const translateY = useRef(new Animated.Value(-24)).current;
  const opacity = useRef(new Animated.Value(0)).current;

  const showToast = useCallback((nextToast: ToastInput) => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    setToast({
      id: Date.now(),
      tone: nextToast.tone ?? 'info',
      title: nextToast.title,
      message: nextToast.message,
    });
  }, []);

  useEffect(() => {
    if (!toast) {
      return;
    }

    Animated.parallel([
      Animated.timing(opacity, {
        toValue: 1,
        duration: 180,
        useNativeDriver: true,
      }),
      Animated.timing(translateY, {
        toValue: 0,
        duration: 180,
        useNativeDriver: true,
      }),
    ]).start();

    timeoutRef.current = setTimeout(() => {
      Animated.parallel([
        Animated.timing(opacity, {
          toValue: 0,
          duration: 180,
          useNativeDriver: true,
        }),
        Animated.timing(translateY, {
          toValue: -24,
          duration: 180,
          useNativeDriver: true,
        }),
      ]).start(({ finished }) => {
        if (finished) {
          setToast(null);
        }
      });
    }, 3200);

    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, [opacity, toast, translateY]);

  const value = useMemo<ToastContextValue>(
    () => ({
      showToast,
    }),
    [showToast]
  );

  const toneStyle =
    toast?.tone === 'success'
      ? styles.success
      : toast?.tone === 'error'
        ? styles.error
        : styles.info;

  return (
    <ToastContext.Provider value={value}>
      {children}
      {toast ? (
        <Animated.View
          pointerEvents="none"
          style={[
            styles.container,
            {
              top: insets.top + tokens.spacing.md,
              opacity,
              transform: [{ translateY }],
            },
          ]}>
          <View style={[styles.toast, toneStyle]}>
            <Text style={styles.title}>{toast.title}</Text>
            {toast.message ? <Text style={styles.message}>{toast.message}</Text> : null}
          </View>
        </Animated.View>
      ) : null}
    </ToastContext.Provider>
  );
}

export function useToast() {
  const context = useContext(ToastContext);

  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }

  return context;
}

const styles = StyleSheet.create({
  container: {
    position: 'absolute',
    left: tokens.spacing.md,
    right: tokens.spacing.md,
    zIndex: 20,
  },
  toast: {
    borderRadius: tokens.radius.md,
    borderWidth: 1,
    paddingHorizontal: tokens.spacing.md,
    paddingVertical: tokens.spacing.md,
    shadowColor: '#000000',
    shadowOpacity: 0.18,
    shadowRadius: 16,
    shadowOffset: { width: 0, height: 10 },
    elevation: 8,
  },
  info: {
    backgroundColor: tokens.colors.surfaceStrong,
    borderColor: tokens.colors.border,
  },
  success: {
    backgroundColor: '#12382A',
    borderColor: tokens.colors.success,
  },
  error: {
    backgroundColor: '#441E25',
    borderColor: tokens.colors.danger,
  },
  title: {
    color: tokens.colors.text,
    fontFamily: tokens.typography.bodyStrong,
    fontSize: 15,
  },
  message: {
    color: tokens.colors.textMuted,
    fontFamily: tokens.typography.body,
    fontSize: 14,
    lineHeight: 20,
  },
});
