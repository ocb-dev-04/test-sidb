import type { ServiceRequestDetailResponse, ServiceRequestStatus, UserRole } from '../types/api';

const STAFF_ROLES: readonly UserRole[] = ['Administrator', 'Analyst'];
const STATUSES_REQUIRING_COMMENT: readonly ServiceRequestStatus[] = ['Resolved', 'Closed'];

export function isAdministrator(role: UserRole | undefined): boolean {
  return role === 'Administrator';
}

export function canManageServiceRequests(role: UserRole | undefined): boolean {
  return role !== undefined && STAFF_ROLES.includes(role);
}

export function canAddInternalComments(role: UserRole | undefined): boolean {
  return canManageServiceRequests(role);
}

// Regla del backend: el solicitante solo corrige su propia solicitud mientras está Registrada.
export function canEditServiceRequest(
  role: UserRole | undefined,
  userId: string | undefined,
  serviceRequest: Pick<ServiceRequestDetailResponse, 'status' | 'requesterId'>,
): boolean {
  if (serviceRequest.status === 'Closed') {
    return false;
  }
  if (canManageServiceRequests(role)) {
    return true;
  }
  return role === 'Requester' && serviceRequest.requesterId === userId && serviceRequest.status === 'Registered';
}

export function transitionRequiresComment(
  currentStatus: ServiceRequestStatus,
  targetStatus: ServiceRequestStatus,
): boolean {
  return STATUSES_REQUIRING_COMMENT.includes(targetStatus) || currentStatus === 'Closed';
}
