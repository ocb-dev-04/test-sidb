export const SERVICE_REQUEST_STATUSES = [
  'Registered',
  'InAnalysis',
  'InProgress',
  'WaitingForRequester',
  'Resolved',
  'Closed',
] as const;
export type ServiceRequestStatus = (typeof SERVICE_REQUEST_STATUSES)[number];

export const SERVICE_REQUEST_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;
export type ServiceRequestPriority = (typeof SERVICE_REQUEST_PRIORITIES)[number];

export const USER_ROLES = ['Administrator', 'Analyst', 'Requester'] as const;
export type UserRole = (typeof USER_ROLES)[number];

export const COMMENT_VISIBILITIES = ['Public', 'Internal'] as const;
export type CommentVisibility = (typeof COMMENT_VISIBILITIES)[number];

export const SERVICE_REQUEST_SORT_FIELDS = ['CreatedOn', 'DueDate', 'Priority', 'Status', 'Code'] as const;
export type ServiceRequestSortField = (typeof SERVICE_REQUEST_SORT_FIELDS)[number];

export const SORT_DIRECTIONS = ['Ascending', 'Descending'] as const;
export type SortDirection = (typeof SORT_DIRECTIONS)[number];

export type NotificationType =
  | 'ServiceRequestCreated'
  | 'ServiceRequestAssigned'
  | 'ServiceRequestStatusChanged'
  | 'ServiceRequestClosed'
  | 'ServiceRequestCommented';

export interface PagedResult<TItem> {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  errors?: Record<string, string[]>;
}

export interface UserResponse {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdOnUtc: string;
}

export interface UserLookupResponse {
  id: string;
  fullName: string;
  role: UserRole;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresOnUtc: string;
  user: UserResponse;
}

export interface AreaResponse {
  id: number;
  name: string;
  isActive: boolean;
}

export interface RequestTypeResponse {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
}

export interface ServiceRequestSummaryResponse {
  id: string;
  code: string;
  title: string;
  priority: ServiceRequestPriority;
  status: ServiceRequestStatus;
  areaId: number;
  areaName: string;
  requestTypeId: number;
  requestTypeName: string;
  requesterId: string;
  requesterName: string;
  assigneeId?: string | null;
  assigneeName?: string | null;
  createdOnUtc: string;
  dueDateUtc: string;
  isOverdue: boolean;
}

export interface StatusHistoryResponse {
  id: string;
  previousStatus: ServiceRequestStatus;
  newStatus: ServiceRequestStatus;
  changedById: string;
  changedByName: string;
  changedOnUtc: string;
  comment?: string | null;
}

export interface CommentResponse {
  id: string;
  text: string;
  visibility: CommentVisibility;
  authorId: string;
  authorName: string;
  createdOnUtc: string;
}

export interface ServiceRequestDetailResponse extends ServiceRequestSummaryResponse {
  description: string;
  evidenceReference?: string | null;
  updatedOnUtc?: string | null;
  resolvedOnUtc?: string | null;
  closedOnUtc?: string | null;
  resolutionComment?: string | null;
  statusHistory: StatusHistoryResponse[];
  comments: CommentResponse[];
  allowedTransitions: ServiceRequestStatus[];
}

export interface ServiceRequestQuery {
  search?: string;
  status?: ServiceRequestStatus;
  priority?: ServiceRequestPriority;
  areaId?: number;
  requestTypeId?: number;
  createdFrom?: string;
  createdTo?: string;
  onlyOverdue?: boolean;
  sortBy?: ServiceRequestSortField;
  sortDirection?: SortDirection;
  page: number;
  pageSize: number;
}

export interface CreateServiceRequestRequest {
  title: string;
  description: string;
  priority: ServiceRequestPriority;
  requestTypeId: number;
  areaId: number;
  evidenceReference?: string | null;
}

export interface ServiceRequestCreatedResponse {
  id: string;
  code: string;
}

export interface UpdateServiceRequestRequest {
  title?: string | null;
  description?: string | null;
  priority?: ServiceRequestPriority | null;
  requestTypeId?: number | null;
  areaId?: number | null;
  evidenceReference?: string | null;
}

export interface ChangeStatusRequest {
  newStatus: ServiceRequestStatus;
  comment?: string | null;
}

export interface AddCommentRequest {
  text: string;
  visibility: CommentVisibility;
}

export interface StatusCountResponse {
  status: ServiceRequestStatus;
  count: number;
}

export interface PriorityCountResponse {
  priority: ServiceRequestPriority;
  count: number;
}

export interface DashboardSummaryResponse {
  totalCount: number;
  openCount: number;
  closedCount: number;
  overdueCount: number;
  byStatus: StatusCountResponse[];
  byPriority: PriorityCountResponse[];
  latestRequests: ServiceRequestSummaryResponse[];
}

export interface NotificationResponse {
  id: string;
  serviceRequestId?: string | null;
  type: NotificationType;
  subject: string;
  message: string;
  createdOnUtc: string;
  readOnUtc?: string | null;
}

export interface GovernmentEntityResponse {
  id: number;
  name: string;
  category: string;
  stateBranch: string;
  sector: string;
}

export interface GovernmentEntityRequest {
  name: string;
  category: string;
  stateBranch: string;
  sector: string;
}

export interface GovernmentEntityFilterValuesResponse {
  categories: string[];
  stateBranches: string[];
  sectors: string[];
}

export interface GovernmentEntityQuery {
  search?: string;
  category?: string;
  stateBranch?: string;
  sector?: string;
  sortDirection?: SortDirection;
  page: number;
  pageSize: number;
}

export interface UserQuery {
  search?: string;
  role?: UserRole;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  fullName: string;
  role: UserRole;
  isActive: boolean;
}

export interface CreateAreaRequest {
  name: string;
}

export interface UpdateAreaRequest {
  name: string;
  isActive: boolean;
}

export interface CreateRequestTypeRequest {
  name: string;
  description: string;
}

export interface UpdateRequestTypeRequest {
  name: string;
  description: string;
  isActive: boolean;
}
