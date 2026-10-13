import { DEMO, expect, instrumentPage, test } from './demo';
import type { Browser, BrowserContext, Page, TestInfo } from '@playwright/test';
import {
  ADMINISTRATOR_EMAIL,
  apiLogin,
  createServiceRequest,
  FIRST_ANALYST_EMAIL,
  FIRST_REQUESTER_EMAIL,
  signIn,
  uniqueSuffix,
} from './support';

const CONFLICT_MESSAGE = /Otro usuario modificó la solicitud/;

const VIDEO_SIZE = DEMO ? { width: 1920, height: 1080 } : { width: 1280, height: 720 };
const openContexts: BrowserContext[] = [];
const pendingTimings: ((testInfo: TestInfo) => Promise<void>)[] = [];

// Dos navegadores independientes (contextos) sobre la misma solicitud, como dos personas a la vez. Se graban a
// mano porque Playwright solo graba el contexto de la fixture `page`.
async function openAs(browser: Browser, email: string, route: string): Promise<Page> {
  const context = await browser.newContext({
    viewport: VIDEO_SIZE,
    recordVideo: { dir: test.info().outputPath('videos'), size: VIDEO_SIZE },
  });
  openContexts.push(context);
  const page = await context.newPage();
  if (DEMO) {
    pendingTimings.push(await instrumentPage(page, email.split('@')[0] ?? email));
  }
  await signIn(page, context.request, email, route);
  return page;
}

test.afterEach(async () => {
  for (const attachTiming of pendingTimings.splice(0)) {
    await attachTiming(test.info());
  }
  const pages = openContexts.flatMap((context) => context.pages());
  await Promise.all(openContexts.splice(0).map((context) => context.close()));
  for (const page of pages) {
    const path = await page.video()?.path();
    if (path) {
      await test.info().attach('video', { path, contentType: 'video/webm' });
    }
  }
});

async function changeStatusToInAnalysis(page: Page) {
  await page.getByLabel(/Nuevo estado/).selectOption('InAnalysis');
  await page.getByRole('button', { name: 'Cambiar estado' }).click();
  await expect(page.getByText('Estado actual: En análisis')).toBeVisible();
}

test.describe('Concurrencia entre dos usuarios', () => {
  let detailRoute: string;

  test.beforeEach(async ({ request }) => {
    const requester = await apiLogin(request, FIRST_REQUESTER_EMAIL);
    const created = await createServiceRequest(request, requester, `E2E Concurrencia ${uniqueSuffix()}`);
    detailRoute = `/solicitudes/${created.id}`;
  });

  test('C1 · asignar con datos viejos avisa y recarga la solicitud', async ({ browser }) => {
    const analyst = await openAs(browser, FIRST_ANALYST_EMAIL, detailRoute);
    const administrator = await openAs(browser, ADMINISTRATOR_EMAIL, detailRoute);
    await expect(administrator.getByText('Estado actual: Registrada')).toBeVisible();

    await changeStatusToInAnalysis(analyst);
    await administrator.getByLabel('Responsable técnico').selectOption({ index: 1 });
    await administrator.getByRole('button', { name: 'Asignar' }).click();

    await expect(administrator.getByText(CONFLICT_MESSAGE).first()).toBeVisible();
    await expect(administrator.getByText('Estado actual: En análisis')).toBeVisible();
  });

  test('C2 · guardar una edición vieja conserva lo escrito y el segundo intento guarda', async ({ browser }) => {
    const newTitle = `E2E Título editado ${uniqueSuffix()}`;
    const analyst = await openAs(browser, FIRST_ANALYST_EMAIL, detailRoute);
    const administrator = await openAs(browser, ADMINISTRATOR_EMAIL, detailRoute);
    await administrator.getByRole('button', { name: 'Editar' }).click();
    const dialog = administrator.getByRole('dialog');
    await dialog.getByLabel(/Título/).fill(newTitle);

    await changeStatusToInAnalysis(analyst);
    await dialog.getByRole('button', { name: 'Guardar cambios' }).click();
    await expect(dialog.getByText(CONFLICT_MESSAGE)).toBeVisible();
    await expect(dialog.getByLabel(/Título/)).toHaveValue(newTitle);

    await dialog.getByRole('button', { name: 'Guardar cambios' }).click();
    await expect(dialog).toBeHidden();
    await expect(administrator.getByRole('heading', { name: new RegExp(newTitle) })).toBeVisible();
  });

  test('C3 · dos comentarios a la vez se guardan ambos', async ({ browser }) => {
    const analyst = await openAs(browser, FIRST_ANALYST_EMAIL, detailRoute);
    const administrator = await openAs(browser, ADMINISTRATOR_EMAIL, detailRoute);
    const analystComment = `Comentario del analista ${uniqueSuffix()}`;
    const administratorComment = `Comentario del administrador ${uniqueSuffix()}`;

    await analyst.getByLabel(/Nuevo comentario/).fill(analystComment);
    await administrator.getByLabel(/Nuevo comentario/).fill(administratorComment);
    await analyst.getByRole('button', { name: 'Agregar comentario' }).click();
    await administrator.getByRole('button', { name: 'Agregar comentario' }).click();

    await expect(administrator.getByText(administratorComment)).toBeVisible();
    await expect(administrator.getByText(analystComment)).toBeVisible();
  });
});
