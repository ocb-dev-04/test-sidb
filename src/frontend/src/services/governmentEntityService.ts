import type {
  GovernmentEntityDataSource,
  GovernmentEntityDataSourceResponse,
  GovernmentEntityFilterValuesResponse,
  GovernmentEntityImportResponse,
  GovernmentEntityQuery,
  GovernmentEntityRequest,
  GovernmentEntityResponse,
  PagedResult,
} from '../types/api';
import { request } from './httpClient';

const BASE_PATH = '/api/entidades-gubernamentales';
const IMPORT_SOURCE: GovernmentEntityDataSource = 'Excel';

export const governmentEntityService = {
  sources: () => request<GovernmentEntityDataSourceResponse[]>(`${BASE_PATH}/fuentes`),
  filterValues: (source: GovernmentEntityDataSource) =>
    request<GovernmentEntityFilterValuesResponse>(`${BASE_PATH}/valores-filtro`, { query: { source } }),
  list: (query: GovernmentEntityQuery) =>
    request<PagedResult<GovernmentEntityResponse>>(BASE_PATH, {
      query: {
        Source: query.source,
        Search: query.search,
        Category: query.category,
        StateBranch: query.stateBranch,
        Sector: query.sector,
        SortDirection: query.sortDirection,
        Page: query.page,
        PageSize: query.pageSize,
      },
    }),
  detail: (id: number, source: GovernmentEntityDataSource) =>
    request<GovernmentEntityResponse>(`${BASE_PATH}/${id}`, { query: { source } }),
  create: (source: GovernmentEntityDataSource, body: GovernmentEntityRequest) =>
    request<GovernmentEntityResponse>(BASE_PATH, { method: 'POST', query: { source }, body }),
  update: (id: number, source: GovernmentEntityDataSource, body: GovernmentEntityRequest) =>
    request<void>(`${BASE_PATH}/${id}`, { method: 'PUT', query: { source }, body }),
  remove: (id: number, source: GovernmentEntityDataSource) =>
    request<void>(`${BASE_PATH}/${id}`, { method: 'DELETE', query: { source } }),
  importFromExcel: (destination: GovernmentEntityDataSource) =>
    request<GovernmentEntityImportResponse>(`${BASE_PATH}/importaciones`, {
      method: 'POST',
      query: { destination, source: IMPORT_SOURCE },
    }),
};
