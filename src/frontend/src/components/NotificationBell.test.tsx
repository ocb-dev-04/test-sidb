import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { buildPage } from '../test/testData';
import type { NotificationResponse } from '../types/api';
import { NotificationBell } from './NotificationBell';

const PAGE_SIZE = 10;
const SECOND_PAGE = 2;
const HTTP_STATUS_NO_CONTENT = 204;

function buildNotification(index: number): NotificationResponse {
  return {
    id: `notificacion-${index}`,
    serviceRequestId: `solicitud-${index}`,
    type: 'ServiceRequestStatusChanged',
    subject: `Aviso ${index}`,
    message: 'La solicitud cambió de estado.',
    createdOnUtc: '2026-10-10T12:00:00Z',
    readOnUtc: null,
  };
}

function registerNotificationHandlers() {
  const notifications = Array.from({ length: PAGE_SIZE + 1 }, (_, index) => buildNotification(index + 1));
  const markedIds: string[] = [];
  let allMarked = false;
  server.use(
    http.get(apiUrl('/api/notificaciones/no-leidas/total'), () =>
      HttpResponse.json(allMarked ? 0 : notifications.length - markedIds.length),
    ),
    http.get(apiUrl('/api/notificaciones'), ({ request }) => {
      const page = Number(new URL(request.url).searchParams.get('Page'));
      const items = notifications.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);
      return HttpResponse.json(buildPage(items, page, PAGE_SIZE, notifications.length));
    }),
    http.patch(apiUrl('/api/notificaciones/leidas'), () => {
      allMarked = true;
      return new HttpResponse(null, { status: HTTP_STATUS_NO_CONTENT });
    }),
    http.patch(apiUrl('/api/notificaciones/:id/leida'), ({ params }) => {
      markedIds.push(String(params.id));
      return new HttpResponse(null, { status: HTTP_STATUS_NO_CONTENT });
    }),
  );
  return markedIds;
}

async function openBell() {
  await userEvent.click(await screen.findByRole('button', { name: `Notificaciones (${PAGE_SIZE + 1} sin leer)` }));
}

describe('NotificationBell', () => {
  it('carga la página siguiente con "Ver más"', async () => {
    registerNotificationHandlers();
    renderWithProviders(<NotificationBell />, { user: buildUser('Requester') });
    await openBell();
    expect(await screen.findByText('Aviso 1')).toBeInTheDocument();
    expect(screen.queryByText(`Aviso ${PAGE_SIZE + 1}`)).not.toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: 'Ver más' }));

    expect(await screen.findByText(`Aviso ${PAGE_SIZE * (SECOND_PAGE - 1) + 1}`)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Ver más' })).not.toBeInTheDocument();
  });

  it('marca todas como leídas y actualiza el contador', async () => {
    registerNotificationHandlers();
    renderWithProviders(<NotificationBell />, { user: buildUser('Requester') });
    await openBell();

    await userEvent.click(await screen.findByRole('button', { name: 'Marcar todas como leídas' }));

    expect(await screen.findByRole('button', { name: 'Notificaciones (0 sin leer)' })).toBeInTheDocument();
  });

  it('marca como leída la notificación al abrir su solicitud', async () => {
    const markedIds = registerNotificationHandlers();
    renderWithProviders(<NotificationBell />, { user: buildUser('Requester') });
    await openBell();
    await screen.findByText('Aviso 1');

    await userEvent.click(screen.getAllByRole('link', { name: 'Ver solicitud' })[0] as HTMLElement);

    await waitFor(() => expect(markedIds).toEqual(['notificacion-1']));
  });
});
