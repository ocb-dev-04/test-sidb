import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { Card } from '../components/Card';
import { FilterPanel } from '../components/FilterPanel';
import { Pagination } from '../components/Pagination';
import { ServiceRequestTable } from '../components/ServiceRequestTable';
import { EmptyState, ErrorState, LoadingState } from '../components/States';
import { useAuth } from '../hooks/useAuth';
import { useClampedPage } from '../hooks/useClampedPage';
import { useAreas, useRequestTypes } from '../hooks/useCatalogs';
import {
  SEARCH_PARAMETER,
  readOption,
  readPositiveInteger,
  useListSearchParameters,
  useSearchText,
} from '../hooks/useListSearchParameters';
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
import type { ServiceRequestQuery, ServiceRequestSortField, SortDirection } from '../types/api';
import { PRIORITY_LABELS, SORT_DIRECTION_LABELS, SORT_FIELD_LABELS, STATUS_LABELS } from '../utils/labels';

const DEFAULT_SORT_FIELD: ServiceRequestSortField = 'CreatedOn';
const DEFAULT_SORT_DIRECTION: SortDirection = 'Descending';
const SORT_FIELD_PARAMETER = 'orden';
const SORT_DIRECTION_PARAMETER = 'direccion';
const SORT_PARAMETERS: readonly string[] = [SORT_FIELD_PARAMETER, SORT_DIRECTION_PARAMETER];

function readFilters(parameters: URLSearchParams): Omit<ServiceRequestQuery, 'page' | 'pageSize'> {
  return {
    search: parameters.get(SEARCH_PARAMETER) ?? undefined,
    status: readOption(SERVICE_REQUEST_STATUSES, parameters.get('estado')),
    priority: readOption(SERVICE_REQUEST_PRIORITIES, parameters.get('prioridad')),
    areaId: readPositiveInteger(parameters.get('area')),
    requestTypeId: readPositiveInteger(parameters.get('tipo')),
    createdFrom: parameters.get('desde') ?? undefined,
    createdTo: parameters.get('hasta') ?? undefined,
    onlyOverdue: parameters.get('vencidas') === 'true',
    sortBy: readOption(SERVICE_REQUEST_SORT_FIELDS, parameters.get(SORT_FIELD_PARAMETER)) ?? DEFAULT_SORT_FIELD,
    sortDirection: readOption(SORT_DIRECTIONS, parameters.get(SORT_DIRECTION_PARAMETER)) ?? DEFAULT_SORT_DIRECTION,
  };
}

export function ServiceRequestListPage() {
  const { user } = useAuth();
  const listParameters = useListSearchParameters(SORT_PARAMETERS);
  const { searchParameters, page, pageSize, activeFilterCount, updateParameters } = listParameters;
  const query = useMemo<ServiceRequestQuery>(
    () => ({ ...readFilters(searchParameters), page, pageSize }),
    [searchParameters, page, pageSize],
  );
  const [searchText, setSearchText] = useSearchText(query.search ?? '', updateParameters);
  const areas = useAreas();
  const requestTypes = useRequestTypes();

  const serviceRequests = useQuery({
    queryKey: QUERY_KEYS.serviceRequestList(query),
    queryFn: () => serviceRequestService.list(query),
    placeholderData: keepPreviousData,
  });
  useClampedPage(serviceRequests.data, listParameters.setPage);

  const hasFilters = activeFilterCount > 0;
  const clearFilters = () => listParameters.clearFilters(SORT_PARAMETERS);

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
            onChange={(event) => updateParameters({ [SORT_FIELD_PARAMETER]: event.target.value })}
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
            onChange={(event) => updateParameters({ [SORT_DIRECTION_PARAMETER]: event.target.value })}
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
          action={
            hasFilters ? undefined : (
              <Link className="button button--primary" to={ROUTES.newServiceRequest}>
                Crear solicitud
              </Link>
            )
          }
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
            onPageChange={listParameters.setPage}
            onPageSizeChange={listParameters.setPageSize}
          />
        </>
      )}
    </Card>
  );
}
