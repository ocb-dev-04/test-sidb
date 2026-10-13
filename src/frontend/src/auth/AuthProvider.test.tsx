import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { act, render, screen, waitFor } from '@testing-library/react';
import { http, HttpResponse } from 'msw';
import { MemoryRouter, Route, Routes, useLocation } from 'react-router-dom';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ToastProvider } from '../components/ToastProvider';
import { useAuth } from '../hooks/useAuth';
import { ROUTES } from '../layout/routes';
import { request } from '../services/httpClient';
import { writeSession } from '../services/sessionStore';
import { buildUser } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { AuthProvider } from './AuthProvider';
import { RequireAuth } from './RequireAuth';

const MILLISECONDS_PER_MINUTE = 60 * 1000;
const SESSION_MINUTES = 6;
const MINUTES_BEFORE_WARNING = 1;
const HTTP_STATUS_UNAUTHORIZED = 401;

function storeSession(expiresInMilliseconds: number) {
  writeSession({
    accessToken: 'token-de-prueba',
    expiresOnUtc: new Date(Date.now() + expiresInMilliseconds).toISOString(),
    user: buildUser('Analyst'),
  });
}

function SessionProbe() {
  const { isAuthenticated, sessionExpired } = useAuth();
  return (
    <span>
      {isAuthenticated ? 'autenticado' : 'sin sesión'} · {sessionExpired ? 'expirada' : 'vigente'}
    </span>
  );
}

function renderAuthProvider(children = <SessionProbe />) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <AuthProvider>{children}</AuthProvider>
      </ToastProvider>
    </QueryClientProvider>,
  );
}

afterEach(() => {
  vi.useRealTimers();
});

describe('AuthProvider', () => {
  it('avisa antes de expirar y cierra la sesión al llegar la hora', () => {
    vi.useFakeTimers();
    storeSession(SESSION_MINUTES * MILLISECONDS_PER_MINUTE);
    renderAuthProvider();
    expect(screen.getByText('autenticado · vigente')).toBeInTheDocument();

    act(() => vi.advanceTimersByTime(MINUTES_BEFORE_WARNING * MILLISECONDS_PER_MINUTE));
    expect(screen.getByRole('status')).toHaveTextContent('Su sesión expirará en 5 minutos');

    act(() => vi.advanceTimersByTime((SESSION_MINUTES - MINUTES_BEFORE_WARNING) * MILLISECONDS_PER_MINUTE));
    expect(screen.getByText('sin sesión · expirada')).toBeInTheDocument();
  });

  it('marca la sesión como expirada cuando la API responde 401', async () => {
    storeSession(SESSION_MINUTES * MILLISECONDS_PER_MINUTE);
    server.use(
      http.get(apiUrl('/api/solicitudes'), () =>
        HttpResponse.json({ status: HTTP_STATUS_UNAUTHORIZED }, { status: HTTP_STATUS_UNAUTHORIZED }),
      ),
    );
    renderAuthProvider();

    await act(() => request('/api/solicitudes').catch(() => undefined));

    await waitFor(() => expect(screen.getByText('sin sesión · expirada')).toBeInTheDocument());
  });

  it('detecta al recargar una sesión guardada que ya expiró', () => {
    storeSession(-MILLISECONDS_PER_MINUTE);

    renderAuthProvider();

    expect(screen.getByText('sin sesión · expirada')).toBeInTheDocument();
  });

  it('el cierre de sesión manual no se muestra como expiración', () => {
    storeSession(SESSION_MINUTES * MILLISECONDS_PER_MINUTE);
    function LogoutButton() {
      const { logout } = useAuth();
      return (
        <button type="button" onClick={logout}>
          Salir
        </button>
      );
    }
    renderAuthProvider(
      <>
        <SessionProbe />
        <LogoutButton />
      </>,
    );

    act(() => screen.getByRole('button', { name: 'Salir' }).click());

    expect(screen.getByText('sin sesión · vigente')).toBeInTheDocument();
  });
});

describe('RequireAuth', () => {
  it('recuerda la ruta con sus filtros al enviar al inicio de sesión', () => {
    const protectedRoute = `${ROUTES.serviceRequests}?estado=InAnalysis&pagina=2`;
    function LoginProbe() {
      const location = useLocation();
      return <span>{(location.state as { from: string }).from}</span>;
    }
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <ToastProvider>
          <AuthProvider>
            <MemoryRouter
              initialEntries={[protectedRoute]}
              future={{ v7_startTransition: true, v7_relativeSplatPath: true }}
            >
              <Routes>
                <Route path={ROUTES.login} element={<LoginProbe />} />
                <Route
                  path={ROUTES.serviceRequests}
                  element={
                    <RequireAuth>
                      <span>Protegida</span>
                    </RequireAuth>
                  }
                />
              </Routes>
            </MemoryRouter>
          </AuthProvider>
        </ToastProvider>
      </QueryClientProvider>,
    );

    expect(screen.getByText(protectedRoute)).toBeInTheDocument();
  });
});
