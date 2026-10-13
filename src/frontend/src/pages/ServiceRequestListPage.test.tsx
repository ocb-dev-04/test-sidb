import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { Link } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { ROUTES } from '../layout/routes';
import { renderWithProviders, buildUser } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { AREAS, REQUEST_TYPES, buildPage, buildServiceRequestSummary } from '../test/testData';
import { ServiceRequestListPage } from './ServiceRequestListPage';

const SEARCH_TEXT = 'acceso';

function registerServiceRequestHandlers(receivedSearches: (string | null)[]) {
  server.use(
    http.get(apiUrl('/api/catalogos/areas'), () => HttpResponse.json(AREAS)),
    http.get(apiUrl('/api/catalogos/tipos-solicitud'), () => HttpResponse.json(REQUEST_TYPES)),
    http.get(apiUrl('/api/solicitudes'), ({ request }) => {
      receivedSearches.push(new URL(request.url).searchParams.get('Search'));
      return HttpResponse.json(buildPage([buildServiceRequestSummary()]));
    }),
  );
}

describe('ServiceRequestListPage', () => {
  it('vacía el buscador cuando la URL pierde el filtro desde fuera', async () => {
    const receivedSearches: (string | null)[] = [];
    registerServiceRequestHandlers(receivedSearches);
    renderWithProviders(
      <>
        <Link to={ROUTES.serviceRequests}>Ir a consulta</Link>
        <ServiceRequestListPage />
      </>,
      { user: buildUser('Administrator'), route: `${ROUTES.serviceRequests}?buscar=${SEARCH_TEXT}` },
    );
    const searchInput = screen.getByRole('searchbox');
    expect(searchInput).toHaveValue(SEARCH_TEXT);
    await waitFor(() => expect(receivedSearches).toContain(SEARCH_TEXT));

    await userEvent.click(screen.getByRole('link', { name: 'Ir a consulta' }));

    expect(searchInput).toHaveValue('');
    await waitFor(() => expect(receivedSearches.at(-1)).toBeNull());
  });

  it('lleva el texto escrito a la URL y a la consulta', async () => {
    const receivedSearches: (string | null)[] = [];
    registerServiceRequestHandlers(receivedSearches);
    renderWithProviders(<ServiceRequestListPage />, {
      user: buildUser('Administrator'),
      route: ROUTES.serviceRequests,
    });

    await userEvent.type(screen.getByRole('searchbox'), SEARCH_TEXT);

    await waitFor(() => expect(receivedSearches.at(-1)).toBe(SEARCH_TEXT));
    expect(screen.getByRole('searchbox')).toHaveValue(SEARCH_TEXT);
  });

  it('ofrece crear una solicitud cuando no hay ninguna y no hay filtros', async () => {
    server.use(
      http.get(apiUrl('/api/catalogos/areas'), () => HttpResponse.json(AREAS)),
      http.get(apiUrl('/api/catalogos/tipos-solicitud'), () => HttpResponse.json(REQUEST_TYPES)),
      http.get(apiUrl('/api/solicitudes'), () => HttpResponse.json(buildPage([]))),
    );

    renderWithProviders(<ServiceRequestListPage />, { user: buildUser('Requester'), route: ROUTES.serviceRequests });

    expect(await screen.findByText('No se encontraron solicitudes')).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Crear solicitud' })).toHaveAttribute('href', ROUTES.newServiceRequest);
  });

  it('con filtros sugiere ajustarlos en lugar de crear', async () => {
    server.use(
      http.get(apiUrl('/api/catalogos/areas'), () => HttpResponse.json(AREAS)),
      http.get(apiUrl('/api/catalogos/tipos-solicitud'), () => HttpResponse.json(REQUEST_TYPES)),
      http.get(apiUrl('/api/solicitudes'), () => HttpResponse.json(buildPage([]))),
    );

    renderWithProviders(<ServiceRequestListPage />, {
      user: buildUser('Requester'),
      route: `${ROUTES.serviceRequests}?estado=Closed`,
    });

    expect(await screen.findByText('Ajuste o limpie los filtros para ver más resultados.')).toBeInTheDocument();
    expect(screen.queryByRole('link', { name: 'Crear solicitud' })).not.toBeInTheDocument();
  });
});
