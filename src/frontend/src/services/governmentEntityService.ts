import type {
  GovernmentEntityFilterValuesResponse,
  GovernmentEntityQuery,
  GovernmentEntityRequest,
  GovernmentEntityResponse,
  PagedResult,
} from '../types/api';
import { request } from './httpClient';

const BASE_PATH = '/api/entidades-gubernamentales';

// La interfaz trabaja siempre con la fuente predeterminada de la API (archivo de texto).
export const governmentEntityService = {
  filterValues: () => request<GovernmentEntityFilterValuesResponse>(`${BASE_PATH}/valores-filtro`),
  list: (query: GovernmentEntityQuery) =>
    request<PagedResult<GovernmentEntityResponse>>(BASE_PATH, {
      query: {
        Search: query.search,
        Category: query.category,
        StateBranch: query.stateBranch,
        Sector: query.sector,
        SortDirection: query.sortDirection,
        Page: query.page,
        PageSize: query.pageSize,
      },
    }),
  detail: (id: number) => request<GovernmentEntityResponse>(`${BASE_PATH}/${id}`),
  create: (body: GovernmentEntityRequest) => request<GovernmentEntityResponse>(BASE_PATH, { method: 'POST', body }),
  update: (id: number, body: GovernmentEntityRequest) => request<void>(`${BASE_PATH}/${id}`, { method: 'PUT', body }),
  remove: (id: number) => request<void>(`${BASE_PATH}/${id}`, { method: 'DELETE' }),
};
