import { expect } from '@playwright/test';
import type { APIRequestContext, Page } from '@playwright/test';

export const DEMO_PASSWORD = 'Demo2026*';
export const ADMINISTRATOR_EMAIL = 'admin@solicitudes.local';
export const FIRST_ANALYST_EMAIL = 'analista1@solicitudes.local';
export const FIRST_REQUESTER_EMAIL = 'solicitante1@solicitudes.local';

const SESSION_STORAGE_KEY = 'sb-solicitudes.session';
const SUPPORT_REQUEST_TYPE_ID = 1;
const FINANCE_AREA_ID = 3;

export interface ApiSession {
  accessToken: string;
  expiresOnUtc: string;
  user: { id: string; fullName: string; email: string; role: string };
}

export interface CreatedServiceRequest {
  id: string;
  code: string;
}

let uniqueCounter = 0;
const cachedSessions = new Map<string, ApiSession>();

export function uniqueSuffix(): string {
  uniqueCounter += 1;
  return `${Date.now()}${uniqueCounter}`;
}

// Un token por usuario para toda la suite: el login tiene límite de intentos por IP.
export async function apiLogin(request: APIRequestContext, email: string): Promise<ApiSession> {
  const cached = cachedSessions.get(email);
  if (cached) {
    return cached;
  }
  const response = await request.post('/api/auth/login', { data: { email, password: DEMO_PASSWORD } });
  expect(response.ok()).toBeTruthy();
  const session = (await response.json()) as ApiSession;
  cachedSessions.set(email, session);
  return session;
}

function authorization(session: ApiSession) {
  return { Authorization: `Bearer ${session.accessToken}` };
}

// Entra sin pasar por el formulario: guarda la sesión como lo hace la aplicación y abre la ruta pedida.
export async function signIn(page: Page, request: APIRequestContext, email: string, route = '/'): Promise<ApiSession> {
  const session = await apiLogin(request, email);
  await storeSession(page, session);
  await page.goto(route);
  return session;
}

export async function storeSession(page: Page, session: ApiSession): Promise<void> {
  await page.goto('/login');
  await page.evaluate(([key, value]) => window.sessionStorage.setItem(key, value), [
    SESSION_STORAGE_KEY,
    JSON.stringify(session),
  ] as const);
}

export async function signInWithForm(page: Page, email: string): Promise<void> {
  await page.getByLabel(/Correo electrónico/).fill(email);
  await page.getByLabel(/Contraseña/).fill(DEMO_PASSWORD);
  await page.getByRole('button', { name: 'Ingresar' }).click();
}

export async function createServiceRequest(
  request: APIRequestContext,
  session: ApiSession,
  title: string,
): Promise<CreatedServiceRequest> {
  const response = await request.post('/api/solicitudes', {
    headers: authorization(session),
    data: {
      title,
      description: 'Creada por las pruebas e2e.',
      priority: 'Medium',
      requestTypeId: SUPPORT_REQUEST_TYPE_ID,
      areaId: FINANCE_AREA_ID,
      evidenceReference: null,
    },
  });
  expect(response.ok()).toBeTruthy();
  return (await response.json()) as CreatedServiceRequest;
}

export async function createUser(
  request: APIRequestContext,
  administrator: ApiSession,
  role: 'Administrator' | 'Analyst' | 'Requester',
): Promise<string> {
  const email = `e2e.${uniqueSuffix()}@solicitudes.local`;
  const response = await request.post('/api/usuarios', {
    headers: authorization(administrator),
    data: { fullName: `Usuario E2E ${role}`, email, password: DEMO_PASSWORD, role },
  });
  expect(response.ok()).toBeTruthy();
  return email;
}

export async function createGovernmentEntity(
  request: APIRequestContext,
  administrator: ApiSession,
  name: string,
): Promise<number> {
  const response = await request.post('/api/entidades-gubernamentales?source=TextFile', {
    headers: authorization(administrator),
    data: { name, category: 'Pruebas E2E', stateBranch: 'Ejecutivo', sector: 'Central' },
  });
  expect(response.ok()).toBeTruthy();
  return ((await response.json()) as { id: number }).id;
}

export async function deleteGovernmentEntity(
  request: APIRequestContext,
  administrator: ApiSession,
  id: number,
): Promise<void> {
  await request.delete(`/api/entidades-gubernamentales/${id}?source=TextFile`, {
    headers: authorization(administrator),
  });
}

export function mainMenu(page: Page) {
  return page.getByRole('navigation', { name: 'Menú principal' });
}
