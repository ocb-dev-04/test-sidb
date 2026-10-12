import type { GovernmentEntityQuery, ServiceRequestQuery, UserQuery } from '../types/api';

export const QUERY_KEYS = {
  dashboard: ['dashboard'] as const,
  areas: (includeInactive: boolean) => ['catalogs', 'areas', includeInactive] as const,
  requestTypes: (includeInactive: boolean) => ['catalogs', 'request-types', includeInactive] as const,
  catalogs: ['catalogs'] as const,
  serviceRequests: ['service-requests'] as const,
  serviceRequestList: (query: ServiceRequestQuery) => ['service-requests', 'list', query] as const,
  serviceRequestDetail: (id: string) => ['service-requests', 'detail', id] as const,
  assignees: ['users', 'assignees'] as const,
  users: ['users'] as const,
  userList: (query: UserQuery) => ['users', 'list', query] as const,
  notifications: ['notifications'] as const,
  unreadNotifications: ['notifications', 'unread-count'] as const,
  notificationList: (page: number) => ['notifications', 'list', page] as const,
  governmentEntities: ['government-entities'] as const,
  governmentEntityFilterValues: ['government-entities', 'filter-values'] as const,
  governmentEntityList: (query: GovernmentEntityQuery) => ['government-entities', 'list', query] as const,
  governmentEntityDetail: (id: number) => ['government-entities', 'detail', id] as const,
};
