import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { ROUTES } from '../layout/routes';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import { apiUrl, server } from '../test/server';
import { buildGovernmentEntity, buildPage } from '../test/testData';
import type { GovernmentEntityDataSourceResponse, GovernmentEntityResponse } from '../types/api';
import { DEFAULT_PAGE_SIZE } from '../utils/pagination';
import { GovernmentEntityListPage } from './GovernmentEntityListPage';

const SECOND_PAGE = 2;
const SOURCES: GovernmentEntityDataSourceResponse[] = [
  { source: 'TextFile', isReadOnly: false, isDefault: true },
  { source: 'Excel', isReadOnly: true, isDefault: false },
];
const FILTER_VALUES = { categories: [], stateBranches: [], sectors: [] };

function buildEntities(count: number): GovernmentEntityResponse[] {
  return Array.from({ length: count }, (_, index) =>
    buildGovernmentEntity({ id: index + 1, name: `Entidad ${index + 1}` }),
  );
}

function registerEntityHandlers(initialEntities: GovernmentEntityResponse[]) {
  let entities = initialEntities;
  server.use(
    http.get(apiUrl('/api/entidades-gubernamentales/fuentes'), () => HttpResponse.json(SOURCES)),
    http.get(apiUrl('/api/entidades-gubernamentales/valores-filtro'), () => HttpResponse.json(FILTER_VALUES)),
    http.get(apiUrl('/api/entidades-gubernamentales'), ({ request }) => {
      const parameters = new URL(request.url).searchParams;
      const page = Number(parameters.get('Page'));
      const pageSize = Number(parameters.get('PageSize'));
      const items = entities.slice((page - 1) * pageSize, page * pageSize);
      return HttpResponse.json(buildPage(items, page, pageSize, entities.length));
    }),
    http.delete(apiUrl('/api/entidades-gubernamentales/:id'), ({ params }) => {
      entities = entities.filter((entity) => String(entity.id) !== params.id);
      return new HttpResponse(null, { status: 204 });
    }),
  );
}

describe('GovernmentEntityListPage', () => {
  it('vuelve a la página anterior al eliminar el único registro de la última página', async () => {
    const entities = buildEntities(DEFAULT_PAGE_SIZE + 1);
    const lastEntity = entities[DEFAULT_PAGE_SIZE] as GovernmentEntityResponse;
    registerEntityHandlers(entities);
    renderWithProviders(<GovernmentEntityListPage />, {
      user: buildUser('Administrator'),
      route: ROUTES.governmentEntities,
    });
    await userEvent.click(await screen.findByRole('button', { name: 'Siguiente' }));
    expect(await screen.findByText(lastEntity.name)).toBeInTheDocument();
    expect(screen.getByText(`Página ${SECOND_PAGE} de ${SECOND_PAGE}`)).toBeInTheDocument();

    await userEvent.click(screen.getByRole('button', { name: `Eliminar ${lastEntity.name}` }));
    await userEvent.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Eliminar' }));

    expect(await screen.findByText('Página 1 de 1')).toBeInTheDocument();
    expect(screen.getByText('Entidad 1')).toBeInTheDocument();
    expect(screen.queryByText('No se encontraron entidades')).not.toBeInTheDocument();
  });

  it('en una fuente de solo lectura permite consultar pero no modificar', async () => {
    registerEntityHandlers(buildEntities(1));

    renderWithProviders(<GovernmentEntityListPage />, {
      user: buildUser('Administrator'),
      route: `${ROUTES.governmentEntities}?fuente=Excel`,
    });

    expect(await screen.findByText('Entidad 1')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Excel/, pressed: true })).toBeInTheDocument();
    expect(screen.getByText(/Puede buscar y filtrar, pero no crear, editar ni/)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Nueva entidad' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Eliminar Entidad 1' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Importar desde Excel' })).toBeDisabled();
  });

  it('el analista consulta sin acciones de administración', async () => {
    registerEntityHandlers(buildEntities(1));

    renderWithProviders(<GovernmentEntityListPage />, { user: buildUser('Analyst'), route: ROUTES.governmentEntities });

    expect(await screen.findByText('Entidad 1')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Eliminar Entidad 1' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Importar desde Excel' })).not.toBeInTheDocument();
  });
});
