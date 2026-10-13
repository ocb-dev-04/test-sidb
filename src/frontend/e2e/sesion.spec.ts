import { expect, test } from '@playwright/test';
import { apiLogin, FIRST_ANALYST_EMAIL, signIn, signInWithForm, storeSession } from './support';

const MILLISECONDS_PER_MINUTE = 60_000;
const WARNING_MINUTES = 5;
const SECONDS_BEFORE_EVENT = 3;
const MILLISECONDS_PER_SECOND = 1000;
const EXPIRED_MESSAGE = 'Su sesión expiró. Inicie sesión de nuevo para continuar.';

function inFuture(milliseconds: number): string {
  return new Date(Date.now() + milliseconds).toISOString();
}

test.describe('Sesión', () => {
  test('S1 · avisa 5 minutos antes de que expire el token', async ({ page, request }) => {
    const session = await apiLogin(request, FIRST_ANALYST_EMAIL);
    const expiresSoon = inFuture(
      WARNING_MINUTES * MILLISECONDS_PER_MINUTE + SECONDS_BEFORE_EVENT * MILLISECONDS_PER_SECOND,
    );
    await storeSession(page, { ...session, expiresOnUtc: expiresSoon });

    await page.goto('/');

    await expect(page.getByText('Su sesión expirará en 5 minutos. Guarde sus cambios.')).toBeVisible();
  });

  test('S2 · al vencer el token cierra la sesión y lo explica', async ({ page, request }) => {
    const session = await apiLogin(request, FIRST_ANALYST_EMAIL);
    await storeSession(page, { ...session, expiresOnUtc: inFuture(SECONDS_BEFORE_EVENT * MILLISECONDS_PER_SECOND) });
    await page.goto('/solicitudes');
    await expect(page.getByRole('heading', { name: 'Consulta de solicitudes', level: 1 })).toBeVisible();

    await expect(page.getByText(EXPIRED_MESSAGE)).toBeVisible();
    await expect(page).toHaveURL(/\/login$/);
  });

  test('S3 · un 401 de la API lleva al login con el aviso', async ({ page, request }) => {
    const session = await apiLogin(request, FIRST_ANALYST_EMAIL);
    await storeSession(page, { ...session, accessToken: `${session.accessToken}alterado` });

    await page.goto('/solicitudes');

    await expect(page.getByText(EXPIRED_MESSAGE)).toBeVisible();
  });

  test('S4 · al recargar con una sesión ya vencida explica que expiró', async ({ page, request }) => {
    const session = await apiLogin(request, FIRST_ANALYST_EMAIL);
    await storeSession(page, { ...session, expiresOnUtc: inFuture(-MILLISECONDS_PER_MINUTE) });

    await page.goto('/');

    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByText(EXPIRED_MESSAGE)).toBeVisible();
  });

  test('S5 · cerrar sesión a mano no se presenta como expiración', async ({ page, request }) => {
    await signIn(page, request, FIRST_ANALYST_EMAIL);

    await page.getByRole('button', { name: 'Cerrar sesión' }).click();

    await expect(page.getByRole('heading', { name: 'Iniciar sesión' })).toBeVisible();
    await expect(page.getByText(EXPIRED_MESSAGE)).toBeHidden();
  });

  test('S6 · tras iniciar sesión vuelve a la ruta pedida con sus filtros', async ({ page }) => {
    const requestedRoute = '/solicitudes?estado=InAnalysis&prioridad=High';
    await page.goto(requestedRoute);
    await expect(page).toHaveURL(/\/login$/);

    await signInWithForm(page, FIRST_ANALYST_EMAIL);

    await expect(page).toHaveURL(new RegExp(`${requestedRoute.replace('?', '\\?')}$`));
    await expect(page.getByRole('combobox', { name: 'Estado', exact: true })).toHaveValue('InAnalysis');
    await expect(page.getByRole('combobox', { name: 'Prioridad', exact: true })).toHaveValue('High');
  });
});
