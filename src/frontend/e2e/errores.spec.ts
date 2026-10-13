import { expect, test } from './demo';
import { ADMINISTRATOR_EMAIL, mainMenu, signIn } from './support';

const SLOW_CHUNK_DELAY_MILLISECONDS = 1500;

test.describe('Errores de pantalla y carga bajo demanda', () => {
  test('E1 · una ruta inexistente muestra "Página no encontrada" con el menú', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/no-existe');

    await expect(page.getByText('La dirección que intentó abrir no existe o fue movida.')).toBeVisible();
    await expect(mainMenu(page)).toBeVisible();
  });

  test('E2 · si no se puede descargar una pantalla muestra el error en español con el menú', async ({
    page,
    request,
    context,
  }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL);
    await expect(page.getByRole('heading', { name: 'Inicio', level: 1 })).toBeVisible();

    await context.setOffline(true);
    await mainMenu(page).getByRole('link', { name: 'Usuarios' }).click();

    await expect(page.getByText('Algo salió mal')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Recargar' })).toBeVisible();
    await expect(mainMenu(page)).toBeVisible();
    await context.setOffline(false);
  });

  test('E3 · mientras llega la pantalla muestra "Cargando pantalla…"', async ({ page, request }) => {
    await page.route('**/assets/CatalogsPage-*.js', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, SLOW_CHUNK_DELAY_MILLISECONDS));
      await route.continue();
    });

    await signIn(page, request, ADMINISTRATOR_EMAIL, '/administracion/catalogos');

    await expect(page.getByText('Cargando pantalla…')).toBeVisible();
    await expect(page.getByRole('tab', { name: 'Áreas' })).toBeVisible();
  });
});
