import { useQueryClient } from '@tanstack/react-query';
import { useCallback, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { authService } from '../services/authService';
import { onUnauthorized } from '../services/httpClient';
import { clearSession, readSession, writeSession } from '../services/sessionStore';
import type { StoredSession } from '../services/sessionStore';
import { AuthContext } from '../context/authContext';
import type { AuthContextValue } from '../context/authContext';
import type { LoginRequest } from '../types/api';

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const [session, setSession] = useState<StoredSession | null>(() => readSession());

  const logout = useCallback(() => {
    clearSession();
    setSession(null);
    queryClient.clear();
  }, [queryClient]);

  useEffect(() => {
    onUnauthorized(logout);
    return () => onUnauthorized(null);
  }, [logout]);

  const login = useCallback(async (credentials: LoginRequest) => {
    const response = await authService.login(credentials);
    const newSession: StoredSession = {
      accessToken: response.accessToken,
      expiresOnUtc: response.expiresOnUtc,
      user: response.user,
    };
    writeSession(newSession);
    setSession(newSession);
    return response.user;
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user: session?.user ?? null, isAuthenticated: session !== null, login, logout }),
    [session, login, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
