import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { buildServiceRequestDetail } from '../test/testData';
import type { UserLookupResponse } from '../types/api';
import { ServiceRequestManagementPanel } from './ServiceRequestManagementPanel';

const VERSION = 'AAAAAAAAB9E=';
const CLOSING_COMMENT = 'Se entregó el acceso solicitado.';
const HTTP_STATUS_NO_CONTENT = 204;
const ASSIGNEE: UserLookupResponse = { id: 'analista-2', fullName: 'José Martínez', role: 'Analyst' };

function registerPanelHandlers(serviceRequestId: string) {
  const received = { statusChanges: [] as unknown[], assignments: [] as unknown[] };
  server.use(
    http.get(apiUrl('/api/usuarios/responsables'), () => HttpResponse.json([ASSIGNEE])),
    http.patch(apiUrl(`/api/solicitudes/${serviceRequestId}/estado`), async ({ request }) => {
      received.statusChanges.push(await request.json());
      return new HttpResponse(null, { status: HTTP_STATUS_NO_CONTENT });
    }),
    http.patch(apiUrl(`/api/solicitudes/${serviceRequestId}/asignacion`), async ({ request }) => {
      received.assignments.push(await request.json());
      return new HttpResponse(null, { status: HTTP_STATUS_NO_CONTENT });
    }),
  );
  return received;
}

describe('ServiceRequestManagementPanel', () => {
  it('exige comentario para cerrar y lo envía con la versión', async () => {
    const serviceRequest = buildServiceRequestDetail({
      status: 'InProgress',
      allowedTransitions: ['WaitingForRequester', 'Resolved', 'Closed'],
      version: VERSION,
    });
    const received = registerPanelHandlers(serviceRequest.id);
    renderWithProviders(<ServiceRequestManagementPanel serviceRequest={serviceRequest} role="Analyst" />, {
      user: buildUser('Analyst'),
    });

    await userEvent.selectOptions(screen.getByLabelText(/Nuevo estado/), 'Closed');
    await userEvent.click(screen.getByRole('button', { name: 'Cambiar estado' }));
    expect(
      await screen.findByText('El comentario es obligatorio para resolver, cerrar o reabrir la solicitud.'),
    ).toBeInTheDocument();
    expect(received.statusChanges).toHaveLength(0);

    await userEvent.type(screen.getByLabelText(/Comentario/), CLOSING_COMMENT);
    await userEvent.click(screen.getByRole('button', { name: 'Cambiar estado' }));

    await waitFor(() =>
      expect(received.statusChanges).toEqual([{ version: VERSION, newStatus: 'Closed', comment: CLOSING_COMMENT }]),
    );
  });

  it('asigna el responsable elegido con la versión', async () => {
    const serviceRequest = buildServiceRequestDetail({ version: VERSION });
    const received = registerPanelHandlers(serviceRequest.id);
    renderWithProviders(<ServiceRequestManagementPanel serviceRequest={serviceRequest} role="Administrator" />, {
      user: buildUser('Administrator'),
    });

    await userEvent.selectOptions(
      await screen.findByRole('combobox', { name: 'Responsable técnico' }),
      await screen.findByRole('option', { name: `${ASSIGNEE.fullName} (Analista)` }),
    );
    await userEvent.click(screen.getByRole('button', { name: 'Asignar' }));

    await waitFor(() => expect(received.assignments).toEqual([{ assigneeId: ASSIGNEE.id, version: VERSION }]));
  });
});
