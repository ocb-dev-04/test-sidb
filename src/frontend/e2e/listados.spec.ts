import { expect, test } from './demo';
import type { ApiSession } from './support';
import {
  ADMINISTRATOR_EMAIL,
  apiLogin,
  createGovernmentEntity,
  createUser,
  deleteGovernmentEntity,
  mainMenu,
  signIn,
  signInWithForm,
  uniqueSuffix,
} from './support';

const PAGE_SIZE = 10;
const SECOND_PAGE = 2;

test.describe('Listados con filtros en la URL', () => {
  test('L1 · el buscador se vacía si el menú quita el filtro de la URL', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/solicitudes');
    const search = page.getByRole('searchbox');
    await search.fill('acceso');
    await expect(page).toHaveURL(/buscar=acceso/);

    await mainMenu(page).getByRole('link', { name: 'Consulta' }).first().click();

    await expect(page).not.toHaveURL(/buscar=/);
    await expect(search).toHaveValue('');
  });

  test('L2 · los filtros sobreviven a ir al detalle y volver atrás', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/solicitudes');
    const status = page.getByRole('combobox', { name: 'Estado', exact: true });
    await status.selectOption('InAnalysis');
    await expect(page).toHaveURL(/estado=InAnalysis/);

    await page.getByRole('table').getByRole('link').first().click();
    await expect(page.getByRole('heading', { name: 'Detalle de solicitud', level: 1 })).toBeVisible();
    await page.goBack();

    await expect(status).toHaveValue('InAnalysis');
  });

  // La sesión vive en sessionStorage (por pestaña): la pestaña nueva pide login y luego abre el enlace compartido.
  test('L3 · la URL con filtros se puede abrir en otra pestaña', async ({ page, request, context }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/solicitudes?prioridad=Critical&orden=DueDate');

    const secondTab = await context.newPage();
    await secondTab.goto(page.url());
    await expect(secondTab).toHaveURL(/\/login$/);
    await signInWithForm(secondTab, ADMINISTRATOR_EMAIL);

    await expect(secondTab.getByRole('combobox', { name: 'Prioridad', exact: true })).toHaveValue('Critical');
    await expect(secondTab.getByRole('combobox', { name: 'Ordenar por', exact: true })).toHaveValue('DueDate');
  });

  test.describe('Entidades con dos páginas de resultados', () => {
    const prefix = `E2E Listado ${uniqueSuffix()}`;
    const createdIds: number[] = [];
    let administrator: ApiSession;

    test.beforeAll(async ({ request }) => {
      administrator = await apiLogin(request, ADMINISTRATOR_EMAIL);
      for (let index = 1; index <= PAGE_SIZE + 1; index++) {
        createdIds.push(
          await createGovernmentEntity(request, administrator, `${prefix} ${String(index).padStart(2, '0')}`),
        );
      }
    });

    test.afterAll(async ({ request }) => {
      for (const id of createdIds) {
        await deleteGovernmentEntity(request, administrator, id);
      }
    });

    test('L4 · al cancelar la edición vuelve a la misma página y filtros', async ({ page, request }) => {
      await signIn(page, request, ADMINISTRATOR_EMAIL, '/entidades-gubernamentales');
      await page.getByRole('searchbox').fill(prefix);
      await expect(page.getByText(`Página 1 de ${SECOND_PAGE}`, { exact: true })).toBeVisible();
      await page.getByRole('button', { name: 'Siguiente' }).click();
      await expect(page.getByText(`Página ${SECOND_PAGE} de ${SECOND_PAGE}`, { exact: true })).toBeVisible();

      await page.getByRole('link', { name: `Editar ${prefix} 11` }).click();
      await expect(page.getByRole('heading', { name: 'Editar entidad gubernamental', level: 1 })).toBeVisible();
      await page.getByRole('button', { name: 'Cancelar' }).click();

      await expect(page).toHaveURL(/pagina=2/);
      await expect(page.getByRole('searchbox')).toHaveValue(prefix);
      await expect(page.getByText(`Página ${SECOND_PAGE} de ${SECOND_PAGE}`, { exact: true })).toBeVisible();
    });

    test('L6 · al borrar el único registro de la última página retrocede', async ({ page, request }) => {
      await signIn(page, request, ADMINISTRATOR_EMAIL, '/entidades-gubernamentales');
      await page.getByRole('searchbox').fill(prefix);
      await expect(page.getByText(`Página 1 de ${SECOND_PAGE}`, { exact: true })).toBeVisible();
      await page.getByRole('button', { name: 'Siguiente' }).click();
      await expect(page.getByText(`Página ${SECOND_PAGE} de ${SECOND_PAGE}`, { exact: true })).toBeVisible();

      await page.getByRole('button', { name: `Eliminar ${prefix} 11` }).click();
      await page.getByRole('dialog').getByRole('button', { name: 'Eliminar' }).click();

      await expect(page.getByText('Página 1 de 1', { exact: true })).toBeVisible();
      await expect(page.getByText(`${prefix} 10`)).toBeVisible();
      await expect(page.getByText('No se encontraron entidades')).toBeHidden();
    });
  });

  test('L5 · los filtros de usuarios sobreviven a recargar y se limpian', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/administracion/usuarios');
    const role = page.getByRole('combobox', { name: 'Rol', exact: true });
    const state = page.getByRole('combobox', { name: 'Estado', exact: true });
    await role.selectOption('Analyst');
    await state.selectOption('activos');

    await page.reload();
    await expect(role).toHaveValue('Analyst');
    await expect(state).toHaveValue('activos');

    await page.getByRole('button', { name: 'Limpiar filtros' }).click();
    await expect(role).toHaveValue('');
    await expect(state).toHaveValue('');
  });

  test('L7 · la lista vacía ofrece crear solo cuando no hay filtros', async ({ page, request }) => {
    const administrator = await apiLogin(request, ADMINISTRATOR_EMAIL);
    const newRequesterEmail = await createUser(request, administrator, 'Requester');
    await signIn(page, request, newRequesterEmail, '/solicitudes');
    const emptyState = page.locator('.state');

    await expect(emptyState.getByRole('link', { name: 'Crear solicitud' })).toBeVisible();

    await page.getByRole('searchbox').fill('no-existe-nada');
    await expect(page.getByText('Ajuste o limpie los filtros para ver más resultados.')).toBeVisible();
    await expect(emptyState.getByRole('link', { name: 'Crear solicitud' })).toBeHidden();
  });

  test('L8 · cambiar la fuente de entidades limpia los filtros', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/entidades-gubernamentales');
    await page.getByRole('searchbox').fill('Ministerio');
    await expect(page).toHaveURL(/buscar=Ministerio/);

    await page.getByRole('button', { name: /SQLite/ }).click();

    await expect(page).toHaveURL(/fuente=Sqlite/);
    await expect(page).not.toHaveURL(/buscar=/);
    await expect(page.getByRole('searchbox')).toHaveValue('');
  });
});
