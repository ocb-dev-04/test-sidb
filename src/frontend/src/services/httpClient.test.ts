import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from './apiError';
import { buildQueryString, onUnauthorized, request } from './httpClient';
import { writeSession } from './sessionStore';

const HTTP_STATUS_OK = 200;
const HTTP_STATUS_UNAUTHORIZED = 401;
const ONE_HOUR_MILLISECONDS = 60 * 60 * 1000;

function jsonResponse(body: unknown, status = HTTP_STATUS_OK): Response {
  return new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });
}

afterEach(() => {
  vi.unstubAllGlobals();
  onUnauthorized(null);
});

describe('httpClient', () => {
  it('omite los parámetros vacíos al construir la consulta', () => {
    expect(buildQueryString({ Search: '', Status: 'InAnalysis', AreaId: undefined, Page: 1, OnlyOverdue: true })).toBe(
      '?Status=InAnalysis&Page=1&OnlyOverdue=true',
    );
  });

  it('adjunta el token Bearer de la sesión', async () => {
    writeSession({
      accessToken: 'token-de-prueba',
      expiresOnUtc: new Date(Date.now() + ONE_HOUR_MILLISECONDS).toISOString(),
      user: { id: '1', fullName: 'Admin', email: 'admin@x', role: 'Administrator', isActive: true, createdOnUtc: '' },
    });
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse(3));
    vi.stubGlobal('fetch', fetchMock);

    await expect(request<number>('/api/notificaciones/no-leidas/total')).resolves.toBe(3);
    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect((init.headers as Record<string, string>).Authorization).toBe('Bearer token-de-prueba');
  });

  it('notifica la expiración de sesión ante un 401 y expone el ProblemDetails', async () => {
    const unauthorizedHandler = vi.fn();
    onUnauthorized(unauthorizedHandler);
    vi.stubGlobal(
      'fetch',
      vi
        .fn()
        .mockResolvedValue(
          jsonResponse({ status: HTTP_STATUS_UNAUTHORIZED, title: 'Unauthorized' }, HTTP_STATUS_UNAUTHORIZED),
        ),
    );

    const failure = request('/api/solicitudes').catch((error: unknown) => error);

    const error = await failure;
    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(HTTP_STATUS_UNAUTHORIZED);
    expect(unauthorizedHandler).toHaveBeenCalledOnce();
  });
});
