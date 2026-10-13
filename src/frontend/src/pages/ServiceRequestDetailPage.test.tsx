import { screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { ROUTES } from '../layout/routes';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { AREAS, REQUEST_TYPES, buildServiceRequestDetail } from '../test/testData';
import type { ServiceRequestDetailResponse } from '../types/api';
import { ServiceRequestDetailPage } from './ServiceRequestDetailPage';

const DETAIL_PATH = '/solicitudes/:id';
const FIRST_VERSION = 'AAAAAAAAB9E=';
const SECOND_VERSION = 'AAAAAAAAB9I=';
const HTTP_STATUS_NO_CONTENT = 204;
const HTTP_STATUS_CONFLICT = 409;
const NEW_TITLE = 'Restablecer acceso al portal de proveedores';

interface Scenario {
  detail: ServiceRequestDetailResponse;
  conflictOnUpdate?: boolean;
}

function registerDetailHandlers({ detail, conflictOnUpdate = false }: Scenario) {
  const requests = { detailReads: 0, updates: [] as unknown[], statusChanges: [] as unknown[] };
  let current = detail;
  server.use(
    http.get(apiUrl('/api/catalogos/areas'), () => HttpResponse.json(AREAS)),
    http.get(apiUrl('/api/catalogos/tipos-solicitud'), () => HttpResponse.json(REQUEST_TYPES)),
    http.get(apiUrl('/api/usuarios/responsables'), () => HttpResponse.json([])),
    http.get(apiUrl(`/api/solicitudes/${detail.id}`), () => {
      requests.detailReads += 1;
      return HttpResponse.json(current);
    }),
    http.patch(apiUrl(`/api/solicitudes/${detail.id}`), async ({ request }) => {
      requests.updates.push(await request.json());
      if (conflictOnUpdate) {
        current = { ...current, title: 'Título cambiado por otro usuario', version: SECOND_VERSION };
        return HttpResponse.json(
          { status: HTTP_STATUS_CONFLICT, code: 'serviceRequest.concurrencyConflict' },
          { status: HTTP_STATUS_CONFLICT },
        );
      }
      return new HttpResponse(null, { status: HTTP_STATUS_NO_CONTENT });
    }),
    http.patch(apiUrl(`/api/solicitudes/${detail.id}/estado`), async ({ request }) => {
      requests.statusChanges.push(await request.json());
      return new HttpResponse(null, { status: HTTP_STATUS_NO_CONTENT });
    }),
  );
  return requests;
}

async function editTitle() {
  await userEvent.click(await screen.findByRole('button', { name: 'Editar' }));
  const dialog = screen.getByRole('dialog');
  const title = within(dialog).getByLabelText(/Título/);
  await userEvent.clear(title);
  await userEvent.type(title, NEW_TITLE);
  await userEvent.click(within(dialog).getByRole('button', { name: 'Guardar cambios' }));
}

describe('ServiceRequestDetailPage', () => {
  it('envía solo los campos cambiados junto con la versión leída', async () => {
    const detail = buildServiceRequestDetail({ requesterId: 'user-Requester', version: FIRST_VERSION });
    const requests = registerDetailHandlers({ detail });
    renderWithProviders(<ServiceRequestDetailPage />, {
      user: buildUser('Requester'),
      route: ROUTES.serviceRequestDetail(detail.id),
      path: DETAIL_PATH,
    });

    await editTitle();

    await waitFor(() => expect(requests.updates).toEqual([{ title: NEW_TITLE, version: FIRST_VERSION }]));
  });

  it('ante un conflicto de versión avisa, recarga la solicitud y conserva lo escrito', async () => {
    const detail = buildServiceRequestDetail({ requesterId: 'user-Requester', version: FIRST_VERSION });
    const requests = registerDetailHandlers({ detail, conflictOnUpdate: true });
    renderWithProviders(<ServiceRequestDetailPage />, {
      user: buildUser('Requester'),
      route: ROUTES.serviceRequestDetail(detail.id),
      path: DETAIL_PATH,
    });

    await editTitle();

    expect(await within(screen.getByRole('dialog')).findByRole('alert')).toHaveTextContent(
      'Otro usuario modificó la solicitud',
    );
    await waitFor(() => expect(requests.detailReads).toBeGreaterThan(1));
    expect(within(screen.getByRole('dialog')).getByLabelText(/Título/)).toHaveValue(NEW_TITLE);
  });

  it('el cambio de estado envía la versión de la solicitud', async () => {
    const detail = buildServiceRequestDetail({ status: 'Registered', version: FIRST_VERSION });
    const requests = registerDetailHandlers({ detail });
    renderWithProviders(<ServiceRequestDetailPage />, {
      user: buildUser('Analyst'),
      route: ROUTES.serviceRequestDetail(detail.id),
      path: DETAIL_PATH,
    });

    await userEvent.click(await screen.findByRole('button', { name: 'Cambiar estado' }));

    await waitFor(() =>
      expect(requests.statusChanges).toEqual([{ version: FIRST_VERSION, newStatus: 'InAnalysis', comment: null }]),
    );
  });
});
