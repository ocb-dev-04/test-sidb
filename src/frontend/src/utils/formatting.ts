const LOCALE = 'es-DO';

const DATE_TIME_FORMAT = new Intl.DateTimeFormat(LOCALE, {
  dateStyle: 'medium',
  timeStyle: 'short',
});

const DATE_FORMAT = new Intl.DateTimeFormat(LOCALE, { dateStyle: 'medium' });

export function formatDateTime(value: string | null | undefined): string {
  return value ? DATE_TIME_FORMAT.format(new Date(value)) : '—';
}

export function formatDate(value: string | null | undefined): string {
  return value ? DATE_FORMAT.format(new Date(value)) : '—';
}

export function formatNumber(value: number): string {
  return value.toLocaleString(LOCALE);
}
