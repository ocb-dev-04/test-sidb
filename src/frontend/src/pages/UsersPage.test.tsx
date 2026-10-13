import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { ROUTES } from '../layout/routes';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { buildPage } from '../test/testData';
import { DEFAULT_PAGE_SIZE } from '../utils/pagination';
import { UsersPage } from './UsersPage';

const SECOND_PAGE = 2;
const TOTAL_USERS = DEFAULT_PAGE_SIZE + 1;

function registerUserHandlers(receivedQueries: URLSearchParams[]) {
  server.use(
    http.get(apiUrl('/api/usuarios'), ({ request }) => {
      const parameters = new URL(request.url).searchParams;
      receivedQueries.push(parameters);
      const page = Number(parameters.get('Page'));
      return HttpResponse.json(buildPage([buildUser('Analyst')], page, DEFAULT_PAGE_SIZE, TOTAL_USERS));
    }),
  );
}

describe('UsersPage', () => {
  it('lee filtros y página de la URL', async () => {
    const receivedQueries: URLSearchParams[] = [];
    registerUserHandlers(receivedQueries);

    renderWithProviders(<UsersPage />, {
      user: buildUser('Administrator'),
      route: `${ROUTES.users}?rol=Analyst&estado=inactivos&pagina=${SECOND_PAGE}`,
    });

    await waitFor(() => expect(receivedQueries).not.toHaveLength(0));
    const query = receivedQueries[0] as URLSearchParams;
    expect(query.get('Role')).toBe('Analyst');
    expect(query.get('IsActive')).toBe('false');
    expect(query.get('Page')).toBe(String(SECOND_PAGE));
    expect(screen.getByLabelText('Rol')).toHaveValue('Analyst');
    expect(screen.getByLabelText('Estado')).toHaveValue('inactivos');
  });

  it('al limpiar filtros vuelve a la primera página sin filtros', async () => {
    const receivedQueries: URLSearchParams[] = [];
    registerUserHandlers(receivedQueries);
    renderWithProviders(<UsersPage />, {
      user: buildUser('Administrator'),
      route: `${ROUTES.users}?rol=Analyst&pagina=${SECOND_PAGE}`,
    });
    await screen.findByText(`Página ${SECOND_PAGE} de ${SECOND_PAGE}`);

    await userEvent.click(screen.getByRole('button', { name: 'Limpiar filtros' }));

    await waitFor(() => expect(receivedQueries.at(-1)?.get('Role')).toBeNull());
    expect(receivedQueries.at(-1)?.get('Page')).toBe('1');
  });
});
