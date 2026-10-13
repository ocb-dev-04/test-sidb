import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { useToast } from '../hooks/useToast';
import { authService } from '../services/authService';
import { onUnauthorized } from '../services/httpClient';
import { clearSession, hasStoredSession, readSession, writeSession } from '../services/sessionStore';
import type { StoredSession } from '../services/sessionStore';
import { AuthContext } from '../context/authContext';
import type { AuthContextValue } from '../context/authContext';
import type { LoginRequest } from '../types/api';

const EXPIRATION_WARNING_MINUTES = 5;
const MILLISECONDS_PER_MINUTE = 60 * 1000;
const EXPIRATION_WARNING_MILLISECONDS = EXPIRATION_WARNING_MINUTES * MILLISECONDS_PER_MINUTE;
// setTimeout se dispara de inmediato si el retraso supera un entero de 32 bits.
const MAXIMUM_TIMER_MILLISECONDS = 2_147_483_647;
const EXPIRATION_WARNING_MESSAGE = `Su sesión expirará en ${EXPIRATION_WARNING_MINUTES} minutos. Guarde sus cambios.`;

function scheduleAt(timestamp: number, callback: () => void): number | undefined {
  const delay = timestamp - Date.now();
  if (delay > MAXIMUM_TIMER_MILLISECONDS) {
    return undefined;
  }
  return window.setTimeout(callback, Math.max(delay, 0));
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [session, setSession] = useState<StoredSession | null>(() => readSession());
  const [sessionExpired, setSessionExpired] = useState(() => {
    const isStoredSessionExpired = session === null && hasStoredSession();
    if (isStoredSessionExpired) {
      clearSession();
    }
    return isStoredSessionExpired;
  });

  const endSession = useCallback(
    (expired: boolean) => {
      clearSession();
      setSession(null);
      setSessionExpired(expired);
      queryClient.clear();
    },
    [queryClient],
  );
  const logout = useCallback(() => endSession(false), [endSession]);
  const expire = useCallback(() => endSession(true), [endSession]);

  useEffect(() => {
    onUnauthorized(expire);
    return () => onUnauthorized(null);
  }, [expire]);

  useEffect(() => {
    if (!session) {
      return;
    }
    const expiresOn = new Date(session.expiresOnUtc).getTime();
    const warningTimer = scheduleAt(expiresOn - EXPIRATION_WARNING_MILLISECONDS, () =>
      showToast(EXPIRATION_WARNING_MESSAGE, 'warning'),
    );
    const expirationTimer = scheduleAt(expiresOn, expire);
    return () => {
      window.clearTimeout(warningTimer);
      window.clearTimeout(expirationTimer);
    };
  }, [session, expire, showToast]);

  const login = useCallback(async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    const newSession: StoredSession = {
      accessToken: response.accessToken,
      expiresOnUtc: response.expiresOnUtc,
      user: response.user,
    };
    writeSession(newSession);
    setSession(newSession);
    setSessionExpired(false);
    return response.user;
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user: session?.user ?? null, isAuthenticated: session !== null, login, logout, sessionExpired }),
    [session, login, logout, sessionExpired],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
