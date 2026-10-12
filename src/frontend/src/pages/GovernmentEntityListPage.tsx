import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { isAdministrator } from '../auth/permissions';
import { Badge } from '../components/Badges';
import { Card } from '../components/Card';
import { FilterPanel } from '../components/FilterPanel';
import { ConfirmDialog } from '../components/Dialog';
import { Pagination } from '../components/Pagination';
import { SourceSelector } from '../components/SourceSelector';
import { Alert, EmptyState, ErrorState, LoadingState } from '../components/States';
import { useAuth } from '../hooks/useAuth';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { PLACEHOLDER_SOURCE, SOURCE_PARAMETER, useGovernmentEntitySource } from '../hooks/useGovernmentEntitySource';
import { useToast } from '../hooks/useToast';
import { ROUTES } from '../layout/routes';
import { SEARCH_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { governmentEntityService } from '../services/governmentEntityService';
import { QUERY_KEYS } from '../services/queryKeys';
import { SORT_DIRECTIONS } from '../types/api';
import type { GovernmentEntityDataSource, GovernmentEntityResponse, SortDirection } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { formatNumber } from '../utils/formatting';
import { dataSourceLabel } from '../utils/labels';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE } from '../utils/pagination';

const READ_ONLY_TOOLTIP = 'La fuente seleccionada es de solo lectura.';
const SORT_LABELS: Record<SortDirection, string> = { Ascending: 'Nombre (A-Z)', Descending: 'Nombre (Z-A)' };

interface EntityFilters {
  category: string;
  stateBranch: string;
  sector: string;
  sortDirection: SortDirection;
}

const EMPTY_FILTERS: EntityFilters = { category: '', stateBranch: '', sector: '', sortDirection: 'Ascending' };

function withSource(path: string, source: GovernmentEntityDataSource): string {
  return `${path}?${SOURCE_PARAMETER}=${source}`;
}

export function GovernmentEntityListPage() {
  const { user } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const sourceSelection = useGovernmentEntitySource();
  const source = sourceSelection.selected?.source;
  const isReadOnly = sourceSelection.selected?.isReadOnly ?? true;
  const isAdmin = isAdministrator(user?.role);
  const canWrite = isAdmin && !isReadOnly;

  const [searchText, setSearchText] = useState('');
  const debouncedSearch = useDebouncedValue(searchText.trim());
  const [filters, setFilters] = useState<EntityFilters>(EMPTY_FILTERS);
  const [page, setPage] = useState<number>(FIRST_PAGE);
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGE_SIZE);
  const [entityToDelete, setEntityToDelete] = useState<GovernmentEntityResponse | null>(null);
  const [isConfirmingImport, setIsConfirmingImport] = useState(false);

  const filterValues = useQuery({
    queryKey: QUERY_KEYS.governmentEntityFilterValues(source ?? PLACEHOLDER_SOURCE),
    queryFn: () => governmentEntityService.filterValues(source as GovernmentEntityDataSource),
    enabled: source !== undefined,
  });

  const listQuery = {
    source: source ?? PLACEHOLDER_SOURCE,
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
    enabled: source !== undefined,
    placeholderData: keepPreviousData,
  });

  const invalidateEntities = () => queryClient.invalidateQueries({ queryKey: QUERY_KEYS.governmentEntities });

  const deleteEntity = useMutation({
    mutationFn: (entity: GovernmentEntityResponse) =>
      governmentEntityService.remove(entity.id, source as GovernmentEntityDataSource),
    onSuccess: async (_, entity) => {
      setEntityToDelete(null);
      showToast(`Se eliminó "${entity.name}".`);
      await invalidateEntities();
    },
  });

  const importEntities = useMutation({
    mutationFn: () => governmentEntityService.importFromExcel(source as GovernmentEntityDataSource),
    onSuccess: async (result) => {
      setIsConfirmingImport(false);
      showToast(
        `Se importaron ${formatNumber(result.importedCount)} entidades desde Excel a ${dataSourceLabel(result.destination)}.`,
      );
      setPage(FIRST_PAGE);
      await invalidateEntities();
    },
  });

  const updateFilter = (changes: Partial<EntityFilters>) => {
    setFilters((current) => ({ ...current, ...changes }));
    setPage(FIRST_PAGE);
  };

  const changeSource = (newSource: GovernmentEntityDataSource) => {
    setFilters(EMPTY_FILTERS);
    setSearchText('');
    setPage(FIRST_PAGE);
    sourceSelection.selectSource(newSource);
  };

  const hasFilters =
    searchText !== '' || filters.category !== '' || filters.stateBranch !== '' || filters.sector !== '';

  if (sourceSelection.isPending) {
    return (
      <Card>
        <LoadingState message="Cargando fuentes de datos…" />
      </Card>
    );
  }
  if (sourceSelection.error || !source) {
    return (
      <Card>
        <ErrorState error={sourceSelection.error} onRetry={sourceSelection.refetch} />
      </Card>
    );
  }

  const readOnlyTitle = isReadOnly ? READ_ONLY_TOOLTIP : undefined;

  return (
    <Card
      title="Entidades gubernamentales"
      subtitle="Listado oficial de entidades gubernamentales de la República Dominicana."
      actions={
        isAdmin && (
          <>
            <span
              className="tooltip-wrapper"
              title={isReadOnly ? 'Seleccione una fuente editable como destino.' : undefined}
            >
              <button
                type="button"
                className="button button--secondary"
                disabled={isReadOnly}
                onClick={() => setIsConfirmingImport(true)}
              >
                Importar desde Excel
              </button>
            </span>
            <span className="tooltip-wrapper" title={readOnlyTitle}>
              {canWrite ? (
                <Link className="button button--accent" to={withSource(ROUTES.newGovernmentEntity, source)}>
                  Nueva entidad
                </Link>
              ) : (
                <button type="button" className="button button--accent" disabled>
                  Nueva entidad
                </button>
              )}
            </span>
          </>
        )
      }
    >
      <div className="stack">
        <SourceSelector sources={sourceSelection.sources} selected={source} onSelect={changeSource} />
        {isReadOnly && (
          <Alert tone="info">
            Está consultando la fuente {dataSourceLabel(source)}. Puede buscar y filtrar, pero no crear, editar ni
            eliminar registros.
          </Alert>
        )}

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
          description={hasFilters ? 'Ajuste o limpie los filtros.' : 'La fuente seleccionada no tiene registros.'}
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
                        <div className="table__actions" title={readOnlyTitle}>
                          {canWrite ? (
                            <Link
                              className="button button--secondary button--small"
                              to={withSource(ROUTES.editGovernmentEntity(entity.id), source)}
                              aria-label={`Editar ${entity.name}`}
                            >
                              Editar
                            </Link>
                          ) : (
                            <button type="button" className="button button--secondary button--small" disabled>
                              Editar
                            </button>
                          )}
                          <button
                            type="button"
                            className="button button--ghost button--small text-danger"
                            disabled={!canWrite}
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
                ¿Desea eliminar <strong>{entityToDelete.name}</strong> de la fuente {dataSourceLabel(source)}? Esta
                acción no se puede deshacer.
              </span>
              {deleteEntity.isError && <Alert tone="error">{errorMessage(deleteEntity.error)}</Alert>}
            </div>
          }
        />
      )}

      {isConfirmingImport && (
        <ConfirmDialog
          title="Importar desde Excel"
          confirmLabel="Importar"
          isProcessing={importEntities.isPending}
          onCancel={() => {
            setIsConfirmingImport(false);
            importEntities.reset();
          }}
          onConfirm={() => importEntities.mutate()}
          message={
            <div className="stack">
              <span>
                Se reemplazará todo el contenido de la fuente <strong>{dataSourceLabel(source)}</strong> con el listado
                del archivo Excel oficial. ¿Desea continuar?
              </span>
              {importEntities.isError && <Alert tone="error">{errorMessage(importEntities.error)}</Alert>}
            </div>
          }
        />
      )}
    </Card>
  );
}
