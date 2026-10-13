import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { useLocation } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import type { ListReturnState } from '../hooks/useListSearchParameters';
import { ROUTES } from '../layout/routes';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { GovernmentEntityFormPage } from './GovernmentEntityFormPage';

const LIST_SEARCH = 'fuente=TextFile&categoria=Ministerio&pagina=2';

function CurrentLocation() {
  const location = useLocation();
  return <span data-testid="ubicacion">{`${location.pathname}${location.search}`}</span>;
}

describe('GovernmentEntityFormPage', () => {
  it('al cancelar vuelve al listado con los mismos filtros y página', async () => {
    server.use(
      http.get(apiUrl('/api/entidades-gubernamentales/fuentes'), () =>
        HttpResponse.json([{ source: 'TextFile', isReadOnly: false, isDefault: true }]),
      ),
      http.get(apiUrl('/api/entidades-gubernamentales/valores-filtro'), () =>
        HttpResponse.json({ categories: [], stateBranches: [], sectors: [] }),
      ),
    );
    const listState: ListReturnState = { listSearch: LIST_SEARCH };
    renderWithProviders(
      <>
        <CurrentLocation />
        <GovernmentEntityFormPage />
      </>,
      { user: buildUser('Administrator'), route: { pathname: ROUTES.newGovernmentEntity, state: listState } },
    );

    await userEvent.click(await screen.findByRole('button', { name: 'Cancelar' }));

    expect(screen.getByTestId('ubicacion')).toHaveTextContent(`${ROUTES.governmentEntities}?${LIST_SEARCH}`);
  });
});
