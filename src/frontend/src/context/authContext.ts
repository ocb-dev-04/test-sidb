import { createContext } from 'react';
import type { LoginRequest, UserResponse } from '../types/api';

export interface AuthContextValue {
  user: UserResponse | null;
  isAuthenticated: boolean;
  login: (credentials: LoginRequest) => Promise<UserResponse>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
