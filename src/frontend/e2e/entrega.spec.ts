import { expect, test } from '@playwright/test';
import { ADMINISTRATOR_EMAIL, mainMenu, signIn } from './support';

const SECURITY_HEADERS = [
  'content-security-policy',
  'x-content-type-options',
  'x-frame-options',
  'referrer-policy',
  'permissions-policy',
  'cross-origin-opener-policy',
];
const FRAME_CHECK_DELAY_MILLISECONDS = 1500;

test.describe('Entrega con nginx', () => {
  test('X1 · la API se llama en el mismo origen y sin preflight CORS', async ({ page, request, baseURL }) => {
    const apiCalls: { url: string; method: string }[] = [];
    page.on('request', (outgoing) => {
      if (outgoing.url().includes('/api/')) {
        apiCalls.push({ url: outgoing.url(), method: outgoing.method() });
      }
    });

    await signIn(page, request, ADMINISTRATOR_EMAIL, '/solicitudes');
    await expect(page.getByRole('table')).toBeVisible();

    expect(apiCalls.length).toBeGreaterThan(0);
    expect(apiCalls.every((call) => call.url.startsWith(`${baseURL}/api/`))).toBeTruthy();
    expect(apiCalls.some((call) => call.method === 'OPTIONS')).toBeFalsy();
  });

  test('X2 · cabeceras de seguridad presentes y ninguna violación de CSP al navegar', async ({ page, request }) => {
    const response = await request.get('/');
    for (const header of SECURITY_HEADERS) {
      expect(response.headers()[header], header).toBeTruthy();
    }

    await page.addInitScript(() => {
      const violations: string[] = [];
      Object.assign(window, { cspViolations: violations });
      document.addEventListener('securitypolicyviolation', (event) => violations.push(event.violatedDirective));
    });
    await signIn(page, request, ADMINISTRATOR_EMAIL);
    for (const label of ['Consulta', 'Usuarios', 'Catálogos']) {
      await mainMenu(page).getByRole('link', { name: label }).first().click();
      await page.waitForLoadState('networkidle');
    }

    const violations = await page.evaluate(() => (window as unknown as { cspViolations: string[] }).cspViolations);
    expect(violations).toEqual([]);
  });

  test('X3 · la aplicación no se puede incrustar en un iframe ajeno', async ({ page, baseURL }) => {
    await page.setContent(`<h1>Sitio ajeno</h1><iframe src="${baseURL}/login" width="600" height="400"></iframe>`);
    await page.waitForTimeout(FRAME_CHECK_DELAY_MILLISECONDS);

    await expect(page.frameLocator('iframe').getByText('Iniciar sesión')).toHaveCount(0);
  });
});
