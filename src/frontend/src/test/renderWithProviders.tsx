import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render } from '@testing-library/react';
import type { ReactElement } from 'react';
import { createMemoryRouter, RouterProvider } from 'react-router-dom';
import { ToastProvider } from '../components/ToastProvider';
import { AuthContext } from '../context/authContext';
import type { AuthContextValue } from '../context/authContext';
import type { UserResponse, UserRole } from '../types/api';

export function buildUser(role: UserRole, overrides: Partial<UserResponse> = {}): UserResponse {
  return {
    id: `user-${role}`,
    fullName: `Usuario ${role}`,
    email: `${role.toLowerCase()}@solicitudes.local`,
    role,
    isActive: true,
    createdOnUtc: '2026-10-01T12:00:00Z',
    ...overrides,
  };
}

interface RenderOptions {
  user?: UserResponse | null;
  login?: AuthContextValue['login'];
  route?: string;
  sessionExpired?: boolean;
}

export function renderWithProviders(element: ReactElement, options: RenderOptions = {}) {
  const { user = null, route = '/' } = options;
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const authValue: AuthContextValue = {
    user,
    isAuthenticated: user !== null,
    login: options.login ?? (async () => user as UserResponse),
    logout: () => undefined,
    sessionExpired: options.sessionExpired ?? false,
  };

  // Router de datos, como en la aplicación: useBlocker no funciona con MemoryRouter.
  const router = createMemoryRouter([{ path: '*', element }], {
    initialEntries: [route],
    future: { v7_relativeSplatPath: true },
  });

  return render(
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider value={authValue}>
        <ToastProvider>
          <RouterProvider router={router} future={{ v7_startTransition: true }} />
        </ToastProvider>
      </AuthContext.Provider>
    </QueryClientProvider>,
  );
}
