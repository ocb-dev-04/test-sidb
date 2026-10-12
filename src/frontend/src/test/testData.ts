import type {
  AreaResponse,
  GovernmentEntityResponse,
  PagedResult,
  RequestTypeResponse,
  ServiceRequestDetailResponse,
  ServiceRequestSummaryResponse,
} from '../types/api';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE } from '../utils/pagination';

export const TECHNOLOGY_AREA: AreaResponse = { id: 1, name: 'Tecnología', isActive: true };
export const AREAS: AreaResponse[] = [TECHNOLOGY_AREA, { id: 2, name: 'Supervisión', isActive: true }];

export const SUPPORT_REQUEST_TYPE: RequestTypeResponse = {
  id: 1,
  name: 'Soporte',
  description: 'Incidencias técnicas',
  isActive: true,
};
export const REQUEST_TYPES: RequestTypeResponse[] = [
  SUPPORT_REQUEST_TYPE,
  { id: 2, name: 'Acceso', description: 'Permisos y cuentas', isActive: true },
];

export function buildPage<TItem>(
  items: TItem[],
  page = FIRST_PAGE,
  pageSize = DEFAULT_PAGE_SIZE,
  totalCount = items.length,
): PagedResult<TItem> {
  const totalPages = Math.ceil(totalCount / pageSize);
  return {
    items,
    page,
    pageSize,
    totalCount,
    totalPages,
    hasNextPage: page < totalPages,
    hasPreviousPage: page > FIRST_PAGE,
  };
}

export function buildServiceRequestSummary(
  overrides: Partial<ServiceRequestSummaryResponse> = {},
): ServiceRequestSummaryResponse {
  return {
    id: 'solicitud-1',
    code: 'SOL-2026-0001',
    title: 'Restablecer acceso al portal',
    priority: 'High',
    status: 'Registered',
    areaId: TECHNOLOGY_AREA.id,
    areaName: TECHNOLOGY_AREA.name,
    requestTypeId: SUPPORT_REQUEST_TYPE.id,
    requestTypeName: SUPPORT_REQUEST_TYPE.name,
    requesterId: 'user-Requester',
    requesterName: 'Usuario Requester',
    assigneeId: null,
    assigneeName: null,
    createdOnUtc: '2026-10-01T12:00:00Z',
    dueDateUtc: '2026-10-03T12:00:00Z',
    isOverdue: false,
    ...overrides,
  };
}

export function buildServiceRequestDetail(
  overrides: Partial<ServiceRequestDetailResponse> = {},
): ServiceRequestDetailResponse {
  return {
    ...buildServiceRequestSummary(),
    description: 'No puedo entrar al portal desde ayer.',
    evidenceReference: null,
    updatedOnUtc: null,
    resolvedOnUtc: null,
    closedOnUtc: null,
    resolutionComment: null,
    statusHistory: [],
    comments: [],
    allowedTransitions: ['InAnalysis'],
    ...overrides,
  };
}

export function buildGovernmentEntity(overrides: Partial<GovernmentEntityResponse> = {}): GovernmentEntityResponse {
  return {
    id: 1,
    name: 'Ministerio de Hacienda',
    category: 'Ministerio',
    stateBranch: 'Ejecutivo',
    sector: 'Central',
    ...overrides,
  };
}
