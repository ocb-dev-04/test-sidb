import { expect, test } from './demo';
import type { APIRequestContext } from '@playwright/test';
import { ADMINISTRATOR_EMAIL, apiLogin, createServiceRequest, createUser, signIn, uniqueSuffix } from './support';

const NOTIFICATION_PAGE_SIZE = 10;

// Un solicitante nuevo recibe una notificación por cada solicitud que registra.
async function requesterWithNotifications(request: APIRequestContext, count: number): Promise<string> {
  const administrator = await apiLogin(request, ADMINISTRATOR_EMAIL);
  const email = await createUser(request, administrator, 'Requester');
  const requester = await apiLogin(request, email);
  for (let index = 1; index <= count; index++) {
    await createServiceRequest(request, requester, `E2E Notificación ${uniqueSuffix()} ${index}`);
  }
  return email;
}

test.describe('Bandeja de notificaciones', () => {
  test('N1 · "Ver más" carga la página siguiente de la bandeja', async ({ page, request }) => {
    const total = NOTIFICATION_PAGE_SIZE + 1;
    const email = await requesterWithNotifications(request, total);
    await signIn(page, request, email);

    await page.getByRole('button', { name: `Notificaciones (${total} sin leer)` }).click();
    const tray = page.getByRole('dialog', { name: 'Notificaciones' });
    await expect(tray.getByRole('listitem')).toHaveCount(NOTIFICATION_PAGE_SIZE);
    await tray.getByRole('button', { name: 'Ver más' }).click();

    await expect(tray.getByRole('listitem')).toHaveCount(total);
    await expect(tray.getByRole('button', { name: 'Ver más' })).toBeHidden();
  });

  test('N2 · "Marcar todas como leídas" deja el contador en cero', async ({ page, request }) => {
    const email = await requesterWithNotifications(request, 2);
    await signIn(page, request, email);

    await page.getByRole('button', { name: 'Notificaciones (2 sin leer)' }).click();
    await page.getByRole('button', { name: 'Marcar todas como leídas' }).click();

    await expect(page.getByRole('button', { name: 'Notificaciones (0 sin leer)' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Marcar todas como leídas' })).toBeHidden();
  });

  test('N3 · "Ver solicitud" abre el detalle y marca la notificación como leída', async ({ page, request }) => {
    const email = await requesterWithNotifications(request, 1);
    await signIn(page, request, email);

    await page.getByRole('button', { name: 'Notificaciones (1 sin leer)' }).click();
    await page.getByRole('link', { name: 'Ver solicitud' }).click();

    await expect(page.getByRole('heading', { name: 'Detalle de solicitud', level: 1 })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Notificaciones (0 sin leer)' })).toBeVisible();
  });
});
