import { expect, test } from './demo';
import { FIRST_ANALYST_EMAIL, mainMenu, signIn } from './support';

const PHONE_VIEWPORT = { width: 375, height: 812 };
const NEGLIGIBLE_SECONDS = 0.01;

test.use({ viewport: PHONE_VIEWPORT, video: { mode: 'on', size: PHONE_VIEWPORT }, isMobile: true, hasTouch: true });

test.describe('Móvil y accesibilidad', () => {
  test('M1 · el menú lleva el foco al abrir y Escape lo cierra devolviéndolo', async ({ page, request }) => {
    await signIn(page, request, FIRST_ANALYST_EMAIL);
    const toggle = page.getByRole('button', { name: 'Abrir menú' });

    await toggle.click();
    await expect(mainMenu(page).getByRole('link', { name: 'Inicio' })).toBeFocused();
    await page.keyboard.press('Escape');

    await expect(toggle).toHaveAttribute('aria-expanded', 'false');
    await expect(toggle).toBeFocused();
  });

  test('M2 · con el menú cerrado sus enlaces quedan fuera del teclado', async ({ page, request }) => {
    await signIn(page, request, FIRST_ANALYST_EMAIL);

    await expect(mainMenu(page).getByRole('link', { name: 'Inicio' })).toBeHidden();
  });

  test('M3 · inicio y bandeja caben en 375 px sin scroll horizontal', async ({ page, request }) => {
    await signIn(page, request, FIRST_ANALYST_EMAIL);
    await expect(page.getByText('Solicitudes por estado')).toBeVisible();

    const overflow = await page.evaluate(
      () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
    );
    expect(overflow).toBeLessThanOrEqual(0);

    await page.getByRole('button', { name: /Notificaciones/ }).click();
    const tray = await page.getByRole('dialog', { name: 'Notificaciones' }).boundingBox();
    expect(tray).not.toBeNull();
    expect(tray?.x ?? -1).toBeGreaterThanOrEqual(0);
    expect((tray?.x ?? 0) + (tray?.width ?? 0)).toBeLessThanOrEqual(PHONE_VIEWPORT.width);
  });

  test('M4 · con "reducir movimiento" el menú aparece sin transición', async ({ page, request }) => {
    await page.emulateMedia({ reducedMotion: 'reduce' });
    await signIn(page, request, FIRST_ANALYST_EMAIL);

    const transitionSeconds = await page
      .locator('#main-sidebar')
      .evaluate((sidebar) => Number.parseFloat(getComputedStyle(sidebar).transitionDuration));

    expect(transitionSeconds).toBeLessThan(NEGLIGIBLE_SECONDS);
  });
});
