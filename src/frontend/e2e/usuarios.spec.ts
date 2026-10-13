import { expect, test } from '@playwright/test';
import { ADMINISTRATOR_EMAIL, apiLogin, createUser, DEMO_PASSWORD, signIn, uniqueSuffix } from './support';

test.describe('Administración de usuarios', () => {
  test('U1 · crea un usuario con rol Analista', async ({ page, request }) => {
    const email = `e2e.analista.${uniqueSuffix()}@solicitudes.local`;
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/administracion/usuarios');

    await page.getByRole('button', { name: 'Nuevo usuario' }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByLabel(/Nombre completo/).fill('Analista de Prueba E2E');
    await dialog.getByLabel(/Correo electrónico/).fill(email);
    await dialog.getByLabel(/Contraseña/).fill(DEMO_PASSWORD);
    await dialog.getByLabel(/Rol/).selectOption('Analyst');
    await dialog.getByRole('button', { name: 'Crear usuario' }).click();

    await expect(dialog).toBeHidden();
    await expect(page.getByText(`Usuario ${email} creado.`)).toBeVisible();
    await page.getByRole('searchbox').fill(email);
    await expect(page.getByRole('row', { name: new RegExp(email) })).toContainText('Analista');
  });

  test('U2 · cambia el rol de un usuario existente', async ({ page, request }) => {
    const administrator = await apiLogin(request, ADMINISTRATOR_EMAIL);
    const email = await createUser(request, administrator, 'Requester');
    await signIn(page, request, ADMINISTRATOR_EMAIL, `/administracion/usuarios?buscar=${encodeURIComponent(email)}`);
    const row = page.getByRole('row', { name: new RegExp(email) });
    await expect(row).toContainText('Solicitante');

    await row.getByRole('button', { name: /Editar/ }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByLabel(/Rol/).selectOption('Analyst');
    await dialog.getByRole('button', { name: 'Guardar cambios' }).click();

    await expect(dialog).toBeHidden();
    await expect(row).toContainText('Analista');
  });

  test('U3 · un correo repetido muestra el error y mantiene el diálogo', async ({ page, request }) => {
    await signIn(page, request, ADMINISTRATOR_EMAIL, '/administracion/usuarios');

    await page.getByRole('button', { name: 'Nuevo usuario' }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByLabel(/Nombre completo/).fill('Correo Repetido');
    await dialog.getByLabel(/Correo electrónico/).fill(ADMINISTRATOR_EMAIL);
    await dialog.getByLabel(/Contraseña/).fill(DEMO_PASSWORD);
    await dialog.getByRole('button', { name: 'Crear usuario' }).click();

    await expect(dialog.getByText('El correo electrónico ya está registrado.')).toBeVisible();
    await expect(dialog).toBeVisible();
  });
});
