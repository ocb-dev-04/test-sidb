import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useEffect, useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { Card } from '../components/Card';
import { FilterPanel } from '../components/FilterPanel';
import { Pagination } from '../components/Pagination';
import { ServiceRequestTable } from '../components/ServiceRequestTable';
import { EmptyState, ErrorState, LoadingState } from '../components/States';
import { useAuth } from '../hooks/useAuth';
import { useAreas, useRequestTypes } from '../hooks/useCatalogs';
import { useDebouncedValue } from '../hooks/useDebouncedValue';
import { ROUTES } from '../layout/routes';
import { canManageServiceRequests } from '../auth/permissions';
import { SEARCH_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { QUERY_KEYS } from '../services/queryKeys';
import { serviceRequestService } from '../services/serviceRequestService';
import {
  SERVICE_REQUEST_PRIORITIES,
  SERVICE_REQUEST_SORT_FIELDS,
  SERVICE_REQUEST_STATUSES,
  SORT_DIRECTIONS,
} from '../types/api';
import type {
  ServiceRequestPriority,
  ServiceRequestQuery,
  ServiceRequestSortField,
  ServiceRequestStatus,
  SortDirection,
} from '../types/api';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE } from '../utils/pagination';
import { PRIORITY_LABELS, SORT_DIRECTION_LABELS, SORT_FIELD_LABELS, STATUS_LABELS } from '../utils/labels';

const DEFAULT_SORT_FIELD: ServiceRequestSortField = 'CreatedOn';
const DEFAULT_SORT_DIRECTION: SortDirection = 'Descending';
const NON_FILTER_PARAMETERS: readonly string[] = ['pagina', 'filas', 'orden', 'direccion'];

function includesValue<TValue extends string>(
  values: readonly TValue[],
  candidate: string | null,
): candidate is TValue {
  return candidate !== null && (values as readonly string[]).includes(candidate);
}

function optionalNumber(value: string | null): number | undefined {
  const parsed = Number(value);
  return value && Number.isInteger(parsed) && parsed > 0 ? parsed : undefined;
}

function readQuery(parameters: URLSearchParams): ServiceRequestQuery {
  const status = parameters.get('estado');
  const priority = parameters.get('prioridad');
  const sortBy = parameters.get('orden');
  const sortDirection = parameters.get('direccion');
  return {
    search: parameters.get('buscar') ?? undefined,
    status: includesValue<ServiceRequestStatus>(SERVICE_REQUEST_STATUSES, status) ? status : undefined,
    priority: includesValue<ServiceRequestPriority>(SERVICE_REQUEST_PRIORITIES, priority) ? priority : undefined,
    areaId: optionalNumber(parameters.get('area')),
    requestTypeId: optionalNumber(parameters.get('tipo')),
    createdFrom: parameters.get('desde') ?? undefined,
    createdTo: parameters.get('hasta') ?? undefined,
    onlyOverdue: parameters.get('vencidas') === 'true',
    sortBy: includesValue<ServiceRequestSortField>(SERVICE_REQUEST_SORT_FIELDS, sortBy) ? sortBy : DEFAULT_SORT_FIELD,
    sortDirection: includesValue<SortDirection>(SORT_DIRECTIONS, sortDirection)
      ? sortDirection
      : DEFAULT_SORT_DIRECTION,
    page: optionalNumber(parameters.get('pagina')) ?? FIRST_PAGE,
    pageSize: optionalNumber(parameters.get('filas')) ?? DEFAULT_PAGE_SIZE,
  };
}

export function ServiceRequestListPage() {
  const { user } = useAuth();
  const [searchParameters, setSearchParameters] = useSearchParams();
  const query = useMemo(() => readQuery(searchParameters), [searchParameters]);
  const [searchText, setSearchText] = useState(query.search ?? '');
  const debouncedSearch = useDebouncedValue(searchText.trim());
  const urlSearch = query.search ?? '';
  const [previousUrlSearch, setPreviousUrlSearch] = useState(urlSearch);
  // Si la URL cambia desde fuera (menú, atrás) el campo la sigue; los cambios propios ya coinciden con el debounce.
  if (urlSearch !== previousUrlSearch) {
    setPreviousUrlSearch(urlSearch);
    if (urlSearch !== debouncedSearch) {
      setSearchText(urlSearch);
    }
  }
  const areas = useAreas();
  const requestTypes = useRequestTypes();

  const updateParameters = (changes: Record<string, string | undefined>, resetPage = true) => {
    setSearchParameters(
      (current) => {
        const next = new URLSearchParams(current);
        Object.entries(changes).forEach(([name, value]) => {
          if (value) {
            next.set(name, value);
          } else {
            next.delete(name);
          }
        });
        if (resetPage) {
          next.delete('pagina');
        }
        return next;
      },
      { replace: true },
    );
  };

  useEffect(() => {
    if ((query.search ?? '') !== debouncedSearch) {
      updateParameters({ buscar: debouncedSearch });
    }
    // Solo debe reaccionar al texto ya estabilizado por el debounce.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedSearch]);

  const serviceRequests = useQuery({
    queryKey: QUERY_KEYS.serviceRequestList(query),
    queryFn: () => serviceRequestService.list(query),
    placeholderData: keepPreviousData,
  });

  const activeFilterCount = [...searchParameters.keys()].filter((key) => !NON_FILTER_PARAMETERS.includes(key)).length;
  const hasFilters = activeFilterCount > 0;

  const clearFilters = () => {
    setSearchText('');
    setSearchParameters(new URLSearchParams(), { replace: true });
  };

  return (
    <Card
      title="Consulta de solicitudes"
      subtitle={
        canManageServiceRequests(user?.role)
          ? 'Solicitudes registradas en el sistema.'
          : 'Solicitudes registradas por usted.'
      }
      actions={
        <Link className="button button--accent" to={ROUTES.newServiceRequest}>
          Nueva solicitud
        </Link>
      }
    >
      <FilterPanel label="Filtros de solicitudes" activeFilterCount={activeFilterCount}>
        <label className="field filters__search">
          <span className="field__label">Buscar</span>
          <input
            className="input"
            type="search"
            placeholder="Código, título o descripción"
            maxLength={SEARCH_MAXIMUM_LENGTH}
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
          />
        </label>
        <label className="field">
          <span className="field__label">Estado</span>
          <select
            className="select"
            value={query.status ?? ''}
            onChange={(event) => updateParameters({ estado: event.target.value })}
          >
            <option value="">Todos</option>
            {SERVICE_REQUEST_STATUSES.map((status) => (
              <option key={status} value={status}>
                {STATUS_LABELS[status]}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field__label">Prioridad</span>
          <select
            className="select"
            value={query.priority ?? ''}
            onChange={(event) => updateParameters({ prioridad: event.target.value })}
          >
            <option value="">Todas</option>
            {SERVICE_REQUEST_PRIORITIES.map((priority) => (
              <option key={priority} value={priority}>
                {PRIORITY_LABELS[priority]}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field__label">Área</span>
          <select
            className="select"
            value={query.areaId ?? ''}
            onChange={(event) => updateParameters({ area: event.target.value })}
          >
            <option value="">Todas</option>
            {areas.data?.map((area) => (
              <option key={area.id} value={area.id}>
                {area.name}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field__label">Tipo de solicitud</span>
          <select
            className="select"
            value={query.requestTypeId ?? ''}
            onChange={(event) => updateParameters({ tipo: event.target.value })}
          >
            <option value="">Todos</option>
            {requestTypes.data?.map((requestType) => (
              <option key={requestType.id} value={requestType.id}>
                {requestType.name}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field__label">Creada desde</span>
          <input
            className="input"
            type="date"
            value={query.createdFrom ?? ''}
            max={query.createdTo}
            onChange={(event) => updateParameters({ desde: event.target.value })}
          />
        </label>
        <label className="field">
          <span className="field__label">Creada hasta</span>
          <input
            className="input"
            type="date"
            value={query.createdTo ?? ''}
            min={query.createdFrom}
            onChange={(event) => updateParameters({ hasta: event.target.value })}
          />
        </label>
        <label className="field">
          <span className="field__label">Ordenar por</span>
          <select
            className="select"
            value={query.sortBy}
            onChange={(event) => updateParameters({ orden: event.target.value })}
          >
            {SERVICE_REQUEST_SORT_FIELDS.map((field) => (
              <option key={field} value={field}>
                {SORT_FIELD_LABELS[field]}
              </option>
            ))}
          </select>
        </label>
        <label className="field">
          <span className="field__label">Dirección</span>
          <select
            className="select"
            value={query.sortDirection}
            onChange={(event) => updateParameters({ direccion: event.target.value })}
          >
            {SORT_DIRECTIONS.map((direction) => (
              <option key={direction} value={direction}>
                {SORT_DIRECTION_LABELS[direction]}
              </option>
            ))}
          </select>
        </label>
        <label className="checkbox">
          <input
            type="checkbox"
            checked={query.onlyOverdue ?? false}
            onChange={(event) => updateParameters({ vencidas: event.target.checked ? 'true' : undefined })}
          />
          Solo vencidas
        </label>
        <div className="filters__actions">
          <button type="button" className="button button--ghost" onClick={clearFilters} disabled={!hasFilters}>
            Limpiar filtros
          </button>
        </div>
      </FilterPanel>

      {serviceRequests.isPending && <LoadingState message="Cargando solicitudes…" />}
      {serviceRequests.isError && (
        <ErrorState error={serviceRequests.error} onRetry={() => serviceRequests.refetch()} />
      )}
      {serviceRequests.data && serviceRequests.data.items.length === 0 && (
        <EmptyState
          title="No se encontraron solicitudes"
          description={hasFilters ? 'Ajuste o limpie los filtros para ver más resultados.' : undefined}
        />
      )}
      {serviceRequests.data && serviceRequests.data.items.length > 0 && (
        <>
          <ServiceRequestTable
            items={serviceRequests.data.items}
            showRequester={canManageServiceRequests(user?.role)}
          />
          <Pagination
            page={serviceRequests.data.page}
            pageSize={serviceRequests.data.pageSize}
            totalCount={serviceRequests.data.totalCount}
            totalPages={serviceRequests.data.totalPages}
            onPageChange={(page) => updateParameters({ pagina: String(page) }, false)}
            onPageSizeChange={(pageSize) => updateParameters({ filas: String(pageSize) })}
          />
        </>
      )}
    </Card>
  );
}
