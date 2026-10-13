import { expect, test } from './demo';
import type { Page } from '@playwright/test';
import {
  ADMINISTRATOR_EMAIL,
  apiLogin,
  createServiceRequest,
  FIRST_REQUESTER_EMAIL,
  mainMenu,
  signIn,
  uniqueSuffix,
} from './support';

const DISCARD_BAR_TEXT = 'Hay cambios sin guardar. ¿Desea descartarlos?';
const BACKDROP_POINT = { x: 5, y: 5 };

test.describe('Diálogos y cambios sin guardar', () => {
  let detailRoute: string;

  test.beforeEach(async ({ request }) => {
    const requester = await apiLogin(request, FIRST_REQUESTER_EMAIL);
    const created = await createServiceRequest(request, requester, `E2E Diálogo ${uniqueSuffix()}`);
    detailRoute = `/solicitudes/${created.id}`;
  });

  async function openEditDialog(page: Page) {
    await page.getByRole('button', { name: 'Editar' }).click();
    return page.getByRole('dialog');
  }

  test('D1 · el diálogo enfoca el primer campo, atrapa el foco y bloquea el scroll', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, detailRoute);

    const dialog = await openEditDialog(page);
    const title = dialog.getByLabel(/Título/);
    const save = dialog.getByRole('button', { name: 'Guardar cambios' });

    await expect(title).toBeFocused();
    await save.focus();
    await page.keyboard.press('Tab');
    await expect(title).toBeFocused();
    await page.keyboard.press('Shift+Tab');
    await expect(save).toBeFocused();
    expect(await page.evaluate(() => document.body.style.overflow)).toBe('hidden');
  });

  test('D2 · Escape con cambios pide confirmar y permite seguir editando', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, detailRoute);
    const dialog = await openEditDialog(page);
    await dialog.getByLabel(/Título/).fill('Título modificado sin guardar');

    await page.keyboard.press('Escape');
    await expect(dialog.getByText(DISCARD_BAR_TEXT)).toBeVisible();
    await dialog.getByRole('button', { name: 'Seguir editando' }).click();
    await expect(dialog.getByText(DISCARD_BAR_TEXT)).toBeHidden();
    await expect(dialog.getByLabel(/Título/)).toHaveValue('Título modificado sin guardar');

    await page.keyboard.press('Escape');
    await dialog.getByRole('button', { name: 'Descartar cambios' }).click();
    await expect(dialog).toBeHidden();
  });

  test('D3 · sin cambios, Escape y clic fuera cierran directo', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, detailRoute);

    const dialog = await openEditDialog(page);
    await page.keyboard.press('Escape');
    await expect(dialog).toBeHidden();

    await openEditDialog(page);
    await page.mouse.click(BACKDROP_POINT.x, BACKDROP_POINT.y);
    await expect(dialog).toBeHidden();
  });

  test('D4 · Cancelar con cambios pide la misma confirmación', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, detailRoute);
    const dialog = await openEditDialog(page);
    await dialog.getByLabel(/Descripción/).fill('Descripción modificada');

    await dialog.getByRole('button', { name: 'Cancelar' }).click();

    await expect(dialog.getByText(DISCARD_BAR_TEXT)).toBeVisible();
  });
});

test.describe('Páginas de alta con cambios sin guardar', () => {
  test('D5 · salir de "Crear solicitud" con datos pide confirmar', async ({ page, request }) => {
    await signIn(page, request, FIRST_REQUESTER_EMAIL, '/solicitudes/nueva');
    await page.getByLabel(/Título/).fill('Solicitud a medio escribir');
    const home = mainMenu(page).getByRole('link', { name: 'Inicio' });

    await home.click();
    const confirmation = page.getByRole('dialog', { name: 'Cambios sin guardar' });
    await expect(confirmation).toBeVisible();
    await confirmation.getByRole('button', { name: 'Seguir editando' }).click();
    await expect(page).toHaveURL(/\/solicitudes\/nueva$/);
    await expect(page.getByLabel(/Título/)).toHaveValue('Solicitud a medio escribir');

    await home.click();
    await page
      .getByRole('dialog', { name: 'Cambios sin guardar' })
      .getByRole('button', { name: 'Descartar cambios' })
      .click();
    await expect(page.getByRole('heading', { name: 'Inicio', level: 1 })).toBeVisible();
  });

  test('D6 · cerrar la pestaña con datos dispara el aviso del navegador', async ({ page, request }) => {
    await signIn(page, request, FIRST_REQUESTER_EMAIL, '/solicitudes/nueva');
    await page.getByLabel(/Título/).fill('Solicitud a medio escribir');
    const dialogTypes: string[] = [];
    page.on('dialog', async (dialog) => {
      dialogTypes.push(dialog.type());
      await dialog.accept();
    });

    await page.close({ runBeforeUnload: true });

    await expect.poll(() => dialogTypes).toContain('beforeunload');
  });

  test('D7 · registrar la solicitud navega al detalle sin pedir confirmación', async ({ page, request }) => {
    await signIn(page, request, FIRST_REQUESTER_EMAIL, '/solicitudes/nueva');
    await page.getByLabel(/Título/).fill(`E2E Alta ${uniqueSuffix()}`);
    await page.getByLabel(/Descripción/).fill('Solicitud registrada desde la prueba e2e.');
    await page.getByLabel(/Área/).selectOption({ index: 1 });
    await page.getByLabel(/Tipo de solicitud/).selectOption({ index: 1 });

    await page.getByRole('button', { name: 'Registrar solicitud' }).click();

    await expect(page.getByRole('heading', { name: 'Detalle de solicitud', level: 1 })).toBeVisible();
    await expect(page.getByRole('dialog', { name: 'Cambios sin guardar' })).toBeHidden();
    await expect(page.getByText(/registrada correctamente/)).toBeVisible();
  });

  test('D8 · cambiar de fuente en "Nueva entidad" no se trata como salir', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/entidades-gubernamentales/nueva');
    await page.getByLabel(/Nombre/).fill('Entidad a medio escribir');

    await page.getByRole('button', { name: /SQLite/ }).click();

    await expect(page).toHaveURL(/fuente=Sqlite/);
    await expect(page.getByRole('dialog', { name: 'Cambios sin guardar' })).toBeHidden();
  });
});
