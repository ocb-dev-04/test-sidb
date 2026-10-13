import type {
  AddCommentRequest,
  ChangeStatusRequest,
  CommentResponse,
  CreateServiceRequestRequest,
  PagedResult,
  ServiceRequestCreatedResponse,
  ServiceRequestDetailResponse,
  ServiceRequestQuery,
  ServiceRequestSummaryResponse,
  UpdateServiceRequestRequest,
} from '../types/api';
import { request } from './httpClient';

const BASE_PATH = '/api/solicitudes';

export const serviceRequestService = {
  list: (query: ServiceRequestQuery) =>
    request<PagedResult<ServiceRequestSummaryResponse>>(BASE_PATH, {
      query: {
        Search: query.search,
        Status: query.status,
        Priority: query.priority,
        AreaId: query.areaId,
        RequestTypeId: query.requestTypeId,
        CreatedFrom: query.createdFrom,
        CreatedTo: query.createdTo,
        OnlyOverdue: query.onlyOverdue || undefined,
        SortBy: query.sortBy,
        SortDirection: query.sortDirection,
        Page: query.page,
        PageSize: query.pageSize,
      },
    }),
  detail: (id: string) => request<ServiceRequestDetailResponse>(`${BASE_PATH}/${id}`),
  create: (body: CreateServiceRequestRequest) =>
    request<ServiceRequestCreatedResponse>(BASE_PATH, { method: 'POST', body }),
  update: (id: string, body: UpdateServiceRequestRequest) =>
    request<void>(`${BASE_PATH}/${id}`, { method: 'PATCH', body }),
  changeStatus: (id: string, body: ChangeStatusRequest) =>
    request<void>(`${BASE_PATH}/${id}/estado`, { method: 'PATCH', body }),
  assign: (id: string, assigneeId: string, version: string) =>
    request<void>(`${BASE_PATH}/${id}/asignacion`, { method: 'PATCH', body: { assigneeId, version } }),
  addComment: (id: string, body: AddCommentRequest) =>
    request<CommentResponse>(`${BASE_PATH}/${id}/comentarios`, { method: 'POST', body }),
};
