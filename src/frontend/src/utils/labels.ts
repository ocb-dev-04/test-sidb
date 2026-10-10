import type {
  CommentVisibility,
  GovernmentEntityDataSource,
  NotificationType,
  ServiceRequestPriority,
  ServiceRequestSortField,
  ServiceRequestStatus,
  SortDirection,
  UserRole,
} from '../types/api';

export const STATUS_LABELS: Record<ServiceRequestStatus, string> = {
  Registered: 'Registrada',
  InAnalysis: 'En análisis',
  InProgress: 'En progreso',
  WaitingForRequester: 'En espera del solicitante',
  Resolved: 'Resuelta',
  Closed: 'Cerrada',
};

export const PRIORITY_LABELS: Record<ServiceRequestPriority, string> = {
  Low: 'Baja',
  Medium: 'Media',
  High: 'Alta',
  Critical: 'Crítica',
};

export const ROLE_LABELS: Record<UserRole, string> = {
  Administrator: 'Administrador',
  Analyst: 'Analista',
  Requester: 'Solicitante',
};

export const VISIBILITY_LABELS: Record<CommentVisibility, string> = {
  Public: 'Público',
  Internal: 'Interno',
};

export const DATA_SOURCE_LABELS: Record<GovernmentEntityDataSource, string> = {
  TextFile: 'Archivo de texto',
  Sqlite: 'SQLite',
  Excel: 'Excel (solo lectura)',
};

export const SORT_FIELD_LABELS: Record<ServiceRequestSortField, string> = {
  CreatedOn: 'Fecha de creación',
  DueDate: 'Fecha compromiso',
  Priority: 'Prioridad',
  Status: 'Estado',
  Code: 'Código',
};

export const SORT_DIRECTION_LABELS: Record<SortDirection, string> = {
  Ascending: 'Ascendente',
  Descending: 'Descendente',
};

export const NOTIFICATION_TYPE_LABELS: Record<NotificationType, string> = {
  ServiceRequestCreated: 'Nueva solicitud',
  ServiceRequestAssigned: 'Asignación',
  ServiceRequestStatusChanged: 'Cambio de estado',
  ServiceRequestClosed: 'Solicitud cerrada',
  ServiceRequestCommented: 'Nuevo comentario',
};

export function statusLabel(status: ServiceRequestStatus): string {
  return STATUS_LABELS[status] ?? status;
}

export function priorityLabel(priority: ServiceRequestPriority): string {
  return PRIORITY_LABELS[priority] ?? priority;
}

export function roleLabel(role: UserRole): string {
  return ROLE_LABELS[role] ?? role;
}

export function visibilityLabel(visibility: CommentVisibility): string {
  return VISIBILITY_LABELS[visibility] ?? visibility;
}

export function dataSourceLabel(source: GovernmentEntityDataSource): string {
  return DATA_SOURCE_LABELS[source] ?? source;
}
