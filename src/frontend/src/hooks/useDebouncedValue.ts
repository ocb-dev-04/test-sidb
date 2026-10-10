import { useEffect, useState } from 'react';

const DEFAULT_DELAY_MILLISECONDS = 400;

export function useDebouncedValue<TValue>(value: TValue, delayMilliseconds = DEFAULT_DELAY_MILLISECONDS): TValue {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timeoutId = window.setTimeout(() => setDebouncedValue(value), delayMilliseconds);
    return () => window.clearTimeout(timeoutId);
  }, [value, delayMilliseconds]);

  return debouncedValue;
}
