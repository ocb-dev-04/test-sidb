import type {
  AreaResponse,
  CreateAreaRequest,
  CreateRequestTypeRequest,
  RequestTypeResponse,
  UpdateAreaRequest,
  UpdateRequestTypeRequest,
} from '../types/api';
import { request } from './httpClient';

export const catalogService = {
  areas: (includeInactive = false) => request<AreaResponse[]>('/api/catalogos/areas', { query: { includeInactive } }),
  createArea: (body: CreateAreaRequest) => request<AreaResponse>('/api/catalogos/areas', { method: 'POST', body }),
  updateArea: (id: number, body: UpdateAreaRequest) =>
    request<void>(`/api/catalogos/areas/${id}`, { method: 'PUT', body }),
  requestTypes: (includeInactive = false) =>
    request<RequestTypeResponse[]>('/api/catalogos/tipos-solicitud', { query: { includeInactive } }),
  createRequestType: (body: CreateRequestTypeRequest) =>
    request<RequestTypeResponse>('/api/catalogos/tipos-solicitud', { method: 'POST', body }),
  updateRequestType: (id: number, body: UpdateRequestTypeRequest) =>
    request<void>(`/api/catalogos/tipos-solicitud/${id}`, { method: 'PUT', body }),
};
