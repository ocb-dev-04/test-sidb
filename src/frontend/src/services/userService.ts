import type {
  CreateUserRequest,
  PagedResult,
  UpdateUserRequest,
  UserLookupResponse,
  UserQuery,
  UserResponse,
} from '../types/api';
import { request } from './httpClient';

export const userService = {
  list: (query: UserQuery) =>
    request<PagedResult<UserResponse>>('/api/usuarios', {
      query: {
        Search: query.search,
        Role: query.role,
        IsActive: query.isActive,
        Page: query.page,
        PageSize: query.pageSize,
      },
    }),
  assignees: () => request<UserLookupResponse[]>('/api/usuarios/responsables'),
  create: (body: CreateUserRequest) => request<UserResponse>('/api/usuarios', { method: 'POST', body }),
  update: (id: string, body: UpdateUserRequest) => request<void>(`/api/usuarios/${id}`, { method: 'PUT', body }),
};
