import type { LoginRequest, LoginResponse, UserResponse } from '../types/api';
import { request } from './httpClient';

export const authService = {
  login: (credentials: LoginRequest) =>
    request<LoginResponse>('/api/auth/login', { method: 'POST', body: credentials, authenticated: false }),
  currentUser: () => request<UserResponse>('/api/auth/me'),
};
