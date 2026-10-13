import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { useLocation } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { ROUTES } from '../layout/routes';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { AREAS, REQUEST_TYPES, SUPPORT_REQUEST_TYPE, TECHNOLOGY_AREA } from '../test/testData';
import { ServiceRequestCreatePage } from './ServiceRequestCreatePage';

const CREATED_ID = 'solicitud-nueva';
const CREATED_CODE = 'SOL-2026-0042';
const TITLE = 'Instalar Power BI';
const DESCRIPTION = 'Necesito Power BI para los tableros del área.';
const HTTP_STATUS_CREATED = 201;
const HTTP_STATUS_BAD_REQUEST = 400;

function CurrentPath() {
  return <span data-testid="ruta">{useLocation().pathname}</span>;
}

function registerCatalogHandlers() {
  server.use(
    http.get(apiUrl('/api/catalogos/areas'), () => HttpResponse.json(AREAS)),
    http.get(apiUrl('/api/catalogos/tipos-solicitud'), () => HttpResponse.json(REQUEST_TYPES)),
  );
}

function renderPage() {
  renderWithProviders(
    <>
      <CurrentPath />
      <ServiceRequestCreatePage />
    </>,
    { user: buildUser('Requester'), route: ROUTES.newServiceRequest },
  );
}

async function fillValidForm() {
  await userEvent.type(screen.getByLabelText(/Título/), TITLE);
  await userEvent.type(screen.getByLabelText(/Descripción/), DESCRIPTION);
  await userEvent.selectOptions(await screen.findByLabelText(/Área/), String(TECHNOLOGY_AREA.id));
  await userEvent.selectOptions(screen.getByLabelText(/Tipo de solicitud/), String(SUPPORT_REQUEST_TYPE.id));
}

describe('ServiceRequestCreatePage', () => {
  it('valida los obligatorios sin llamar a la API', async () => {
    registerCatalogHandlers();
    renderPage();

    await userEvent.click(screen.getByRole('button', { name: 'Registrar solicitud' }));

    expect(await screen.findByText('El título es obligatorio.')).toBeInTheDocument();
    expect(screen.getByText('La descripción es obligatoria.')).toBeInTheDocument();
    expect(screen.getByText('Seleccione el área.')).toBeInTheDocument();
  });

  it('registra la solicitud y abre su detalle', async () => {
    registerCatalogHandlers();
    const receivedBodies: unknown[] = [];
    server.use(
      http.post(apiUrl('/api/solicitudes'), async ({ request }) => {
        receivedBodies.push(await request.json());
        return HttpResponse.json({ id: CREATED_ID, code: CREATED_CODE }, { status: HTTP_STATUS_CREATED });
      }),
    );
    renderPage();
    await fillValidForm();

    await userEvent.click(screen.getByRole('button', { name: 'Registrar solicitud' }));

    await waitFor(() => expect(screen.getByTestId('ruta')).toHaveTextContent(ROUTES.serviceRequestDetail(CREATED_ID)));
    expect(receivedBodies).toEqual([
      {
        title: TITLE,
        description: DESCRIPTION,
        priority: 'Medium',
        areaId: TECHNOLOGY_AREA.id,
        requestTypeId: SUPPORT_REQUEST_TYPE.id,
        evidenceReference: null,
      },
    ]);
    expect(screen.getByRole('status')).toHaveTextContent(`Solicitud ${CREATED_CODE} registrada correctamente.`);
  });

  it('muestra junto al campo el error de validación que devuelve la API', async () => {
    registerCatalogHandlers();
    server.use(
      http.post(apiUrl('/api/solicitudes'), () =>
        HttpResponse.json(
          {
            status: HTTP_STATUS_BAD_REQUEST,
            code: 'validation.failed',
            errors: { Title: ["'Title' must be 200 characters or fewer."] },
          },
          { status: HTTP_STATUS_BAD_REQUEST },
        ),
      ),
    );
    renderPage();
    await fillValidForm();

    await userEvent.click(screen.getByRole('button', { name: 'Registrar solicitud' }));

    expect(await screen.findByText('Debe tener como máximo 200 caracteres.')).toBeInTheDocument();
    expect(screen.getByTestId('ruta')).toHaveTextContent(ROUTES.newServiceRequest);
  });
});
