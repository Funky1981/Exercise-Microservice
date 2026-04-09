import { Platform } from 'react-native';

export function blurActiveElementOnWeb() {
  if (Platform.OS !== 'web') {
    return;
  }

  const activeElement = document.activeElement;

  if (activeElement instanceof HTMLElement) {
    activeElement.blur();
  }
}
