import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { isAdministrator } from '../auth/permissions';
import { Badge } from '../components/Badges';
import { Card } from '../components/Card';
import { FilterPanel } from '../components/FilterPanel';
import { ConfirmDialog } from '../components/Dialog';
import { Pagination } from '../components/Pagination';
import { Alert, EmptyState, ErrorState, LoadingState } from '../components/States';
import { useAuth } from '../hooks/useAuth';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { useToast } from '../hooks/useToast';
import { ROUTES } from '../layout/routes';
import { SEARCH_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { governmentEntityService } from '../services/governmentEntityService';
import { QUERY_KEYS } from '../services/queryKeys';
import { SORT_DIRECTIONS } from '../types/api';
import type { GovernmentEntityResponse, SortDirection } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE } from '../utils/pagination';

const SORT_LABELS: Record<SortDirection, string> = { Ascending: 'Nombre (A-Z)', Descending: 'Nombre (Z-A)' };

interface EntityFilters {
  category: string;
  stateBranch: string;
  sector: string;
  sortDirection: SortDirection;
}

const EMPTY_FILTERS: EntityFilters = { category: '', stateBranch: '', sector: '', sortDirection: 'Ascending' };

export function GovernmentEntityListPage() {
  const { user } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const isAdmin = isAdministrator(user?.role);

  const [searchText, setSearchText] = useState('');
  const debouncedSearch = useDebouncedValue(searchText.trim());
  const [filters, setFilters] = useState<EntityFilters>(EMPTY_FILTERS);
  const [page, setPage] = useState<number>(FIRST_PAGE);
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGE_SIZE);
  const [entityToDelete, setEntityToDelete] = useState<GovernmentEntityResponse | null>(null);

  const filterValues = useQuery({
    queryKey: QUERY_KEYS.governmentEntityFilterValues,
    queryFn: governmentEntityService.filterValues,
  });

  const listQuery = {
    search: debouncedSearch,
    category: filters.category,
    stateBranch: filters.stateBranch,
    sector: filters.sector,
    sortDirection: filters.sortDirection,
    page,
    pageSize,
  } as const;

  const entities = useQuery({
    queryKey: QUERY_KEYS.governmentEntityList(listQuery),
    queryFn: () => governmentEntityService.list(listQuery),
    placeholderData: keepPreviousData,
  });

  const deleteEntity = useMutation({
    mutationFn: (entity: GovernmentEntityResponse) => governmentEntityService.remove(entity.id),
    onSuccess: async (_, entity) => {
      setEntityToDelete(null);
      showToast(`Se eliminó "${entity.name}".`);
      await queryClient.invalidateQueries({ queryKey: QUERY_KEYS.governmentEntities });
    },
  });

  const updateFilter = (changes: Partial<EntityFilters>) => {
    setFilters((current) => ({ ...current, ...changes }));
    setPage(FIRST_PAGE);
  };

  const hasFilters =
    searchText !== '' || filters.category !== '' || filters.stateBranch !== '' || filters.sector !== '';

  return (
    <Card
      title="Entidades gubernamentales"
      subtitle="Listado oficial de entidades gubernamentales de la República Dominicana."
      actions={
        isAdmin && (
          <Link className="button button--accent" to={ROUTES.newGovernmentEntity}>
            Nueva entidad
          </Link>
        )
      }
    >
      <div className="stack">
        <FilterPanel
          label="Filtros de entidades"
          activeFilterCount={[searchText, filters.category, filters.stateBranch, filters.sector].filter(Boolean).length}
        >
          <label className="field filters__search">
            <span className="field__label">Buscar por nombre</span>
            <input
              className="input"
              type="search"
              placeholder="Ej.: Ministerio de Hacienda"
              maxLength={SEARCH_MAXIMUM_LENGTH}
              value={searchText}
              onChange={(event) => {
                setSearchText(event.target.value);
                setPage(FIRST_PAGE);
              }}
            />
          </label>
          <label className="field">
            <span className="field__label">Categoría</span>
            <select
              className="select"
              value={filters.category}
              onChange={(event) => updateFilter({ category: event.target.value })}
            >
              <option value="">Todas</option>
              {filterValues.data?.categories.map((category) => (
                <option key={category} value={category}>
                  {category}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span className="field__label">Poder del Estado</span>
            <select
              className="select"
              value={filters.stateBranch}
              onChange={(event) => updateFilter({ stateBranch: event.target.value })}
            >
              <option value="">Todos</option>
              {filterValues.data?.stateBranches.map((stateBranch) => (
                <option key={stateBranch} value={stateBranch}>
                  {stateBranch}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span className="field__label">Sector</span>
            <select
              className="select"
              value={filters.sector}
              onChange={(event) => updateFilter({ sector: event.target.value })}
            >
              <option value="">Todos</option>
              {filterValues.data?.sectors.map((sector) => (
                <option key={sector} value={sector}>
                  {sector}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span className="field__label">Orden</span>
            <select
              className="select"
              value={filters.sortDirection}
              onChange={(event) => updateFilter({ sortDirection: event.target.value as SortDirection })}
            >
              {SORT_DIRECTIONS.map((direction) => (
                <option key={direction} value={direction}>
                  {SORT_LABELS[direction]}
                </option>
              ))}
            </select>
          </label>
          <div className="filters__actions">
            <button
              type="button"
              className="button button--ghost"
              disabled={!hasFilters}
              onClick={() => {
                setSearchText('');
                updateFilter(EMPTY_FILTERS);
              }}
            >
              Limpiar filtros
            </button>
          </div>
        </FilterPanel>
      </div>

      {deleteEntity.isError && !entityToDelete && <Alert tone="error">{errorMessage(deleteEntity.error)}</Alert>}
      {entities.isPending && <LoadingState message="Cargando entidades…" />}
      {entities.isError && <ErrorState error={entities.error} onRetry={() => entities.refetch()} />}
      {entities.data && entities.data.items.length === 0 && (
        <EmptyState
          title="No se encontraron entidades"
          description={hasFilters ? 'Ajuste o limpie los filtros.' : 'Todavía no hay entidades registradas.'}
        />
      )}
      {entities.data && entities.data.items.length > 0 && (
        <>
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th scope="col">Nombre</th>
                  <th scope="col">Categoría</th>
                  <th scope="col">Poder del Estado</th>
                  <th scope="col">Sector</th>
                  {isAdmin && (
                    <th scope="col">
                      <span className="visually-hidden">Acciones</span>
                    </th>
                  )}
                </tr>
              </thead>
              <tbody>
                {entities.data.items.map((entity) => (
                  <tr key={entity.id}>
                    <td>
                      <strong>{entity.name}</strong>
                    </td>
                    <td>
                      <Badge tone="info">{entity.category}</Badge>
                    </td>
                    <td>{entity.stateBranch}</td>
                    <td>{entity.sector}</td>
                    {isAdmin && (
                      <td>
                        <div className="table__actions">
                          <Link
                            className="button button--secondary button--small"
                            to={ROUTES.editGovernmentEntity(entity.id)}
                            aria-label={`Editar ${entity.name}`}
                          >
                            Editar
                          </Link>
                          <button
                            type="button"
                            className="button button--ghost button--small text-danger"
                            aria-label={`Eliminar ${entity.name}`}
                            onClick={() => setEntityToDelete(entity)}
                          >
                            Eliminar
                          </button>
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <Pagination
            page={entities.data.page}
            pageSize={entities.data.pageSize}
            totalCount={entities.data.totalCount}
            totalPages={entities.data.totalPages}
            onPageChange={setPage}
            onPageSizeChange={(newPageSize) => {
              setPageSize(newPageSize);
              setPage(FIRST_PAGE);
            }}
          />
        </>
      )}

      {entityToDelete && (
        <ConfirmDialog
          title="Eliminar entidad"
          confirmLabel="Eliminar"
          isProcessing={deleteEntity.isPending}
          onCancel={() => {
            setEntityToDelete(null);
            deleteEntity.reset();
          }}
          onConfirm={() => deleteEntity.mutate(entityToDelete)}
          message={
            <div className="stack">
              <span>
                ¿Desea eliminar <strong>{entityToDelete.name}</strong>? Esta acción no se puede deshacer.
              </span>
              {deleteEntity.isError && <Alert tone="error">{errorMessage(deleteEntity.error)}</Alert>}
            </div>
          }
        />
      )}
    </Card>
  );
}
