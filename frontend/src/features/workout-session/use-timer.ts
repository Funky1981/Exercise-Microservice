import { useCallback, useEffect, useRef, useState } from 'react';
import { AppState, type AppStateStatus } from 'react-native';

/**
 * Timestamp-based timer that survives app backgrounding.
 *
 * - Uses Date.now() for elapsed calculation (no setInterval drift).
 * - Recalculates on AppState change so time keeps counting while backgrounded.
 * - A 60fps requestAnimationFrame loop drives UI updates while active.
 */
export function useTimer() {
  const [startedAt, setStartedAt] = useState<number | null>(null);
  const [elapsedBeforePause, setElapsedBeforePause] = useState(0);
  const [isRunning, setIsRunning] = useState(false);
  const [displayMs, setDisplayMs] = useState(0);
  const frameRef = useRef<number | null>(null);

  const calcElapsed = useCallback(() => {
    if (!isRunning || startedAt === null) return elapsedBeforePause;
    return elapsedBeforePause + (Date.now() - startedAt);
  }, [isRunning, startedAt, elapsedBeforePause]);

  // Animation frame loop for smooth UI updates
  useEffect(() => {
    if (!isRunning) {
      setDisplayMs(calcElapsed());
      return;
    }

    function tick() {
      setDisplayMs(elapsedBeforePause + (Date.now() - (startedAt ?? Date.now())));
      frameRef.current = requestAnimationFrame(tick);
    }

    frameRef.current = requestAnimationFrame(tick);

    return () => {
      if (frameRef.current !== null) {
        cancelAnimationFrame(frameRef.current);
      }
    };
  }, [isRunning, startedAt, elapsedBeforePause, calcElapsed]);

  // Recalculate when app returns to foreground
  useEffect(() => {
    function handleAppState(status: AppStateStatus) {
      if (status === 'active' && isRunning) {
        setDisplayMs(calcElapsed());
      }
    }

    const subscription = AppState.addEventListener('change', handleAppState);
    return () => subscription.remove();
  }, [isRunning, calcElapsed]);

  const start = useCallback(() => {
    setStartedAt(Date.now());
    setIsRunning(true);
  }, []);

  const pause = useCallback(() => {
    setElapsedBeforePause((prev) => prev + (Date.now() - (startedAt ?? Date.now())));
    setStartedAt(null);
    setIsRunning(false);
  }, [startedAt]);

  const resume = useCallback(() => {
    setStartedAt(Date.now());
    setIsRunning(true);
  }, []);

  const reset = useCallback(() => {
    setStartedAt(null);
    setElapsedBeforePause(0);
    setIsRunning(false);
    setDisplayMs(0);
    if (frameRef.current !== null) {
      cancelAnimationFrame(frameRef.current);
    }
  }, []);

  /** Restore timer state from persisted session (e.g. after app kill). */
  const restore = useCallback(
    (savedStartedAt: number | null, savedElapsedBeforePause: number, wasRunning: boolean) => {
      setStartedAt(savedStartedAt);
      setElapsedBeforePause(savedElapsedBeforePause);
      setIsRunning(wasRunning);
    },
    []
  );

  const elapsedSeconds = Math.floor(displayMs / 1000);

  return {
    isRunning,
    displayMs,
    elapsedSeconds,
    startedAt,
    elapsedBeforePause,
    start,
    pause,
    resume,
    reset,
    restore,
  };
}
