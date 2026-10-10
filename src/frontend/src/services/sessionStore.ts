import type { UserResponse } from '../types/api';

const SESSION_STORAGE_KEY = 'sb-solicitudes.session';

export interface StoredSession {
  accessToken: string;
  expiresOnUtc: string;
  user: UserResponse;
}

export function readSession(): StoredSession | null {
  const serialized = sessionStorage.getItem(SESSION_STORAGE_KEY);
  if (!serialized) {
    return null;
  }

  try {
    const session = JSON.parse(serialized) as StoredSession;
    return new Date(session.expiresOnUtc).getTime() > Date.now() ? session : null;
  } catch {
    return null;
  }
}

export function writeSession(session: StoredSession): void {
  sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  sessionStorage.removeItem(SESSION_STORAGE_KEY);
}
