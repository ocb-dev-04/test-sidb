import type { NotificationResponse, PagedResult } from '../types/api';
import { request } from './httpClient';

export const notificationService = {
  list: (page: number, pageSize: number, onlyUnread = false) =>
    request<PagedResult<NotificationResponse>>('/api/notificaciones', {
      query: { OnlyUnread: onlyUnread || undefined, Page: page, PageSize: pageSize },
    }),
  unreadCount: () => request<number>('/api/notificaciones/no-leidas/total'),
  markAsRead: (id: string) => request<void>(`/api/notificaciones/${id}/leida`, { method: 'PATCH' }),
};
