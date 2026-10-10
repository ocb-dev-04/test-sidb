import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { notificationService } from '../services/notificationService';
import { QUERY_KEYS } from '../services/queryKeys';

const UNREAD_POLLING_MILLISECONDS = 60 * 1000;
const NOTIFICATION_PAGE_SIZE = 10;
const FIRST_NOTIFICATION_PAGE = 1;

export function useUnreadNotificationCount() {
  return useQuery({
    queryKey: QUERY_KEYS.unreadNotifications,
    queryFn: notificationService.unreadCount,
    refetchInterval: UNREAD_POLLING_MILLISECONDS,
  });
}

export function useNotificationList(enabled: boolean) {
  return useQuery({
    queryKey: QUERY_KEYS.notificationList(FIRST_NOTIFICATION_PAGE),
    queryFn: () => notificationService.list(FIRST_NOTIFICATION_PAGE, NOTIFICATION_PAGE_SIZE),
    enabled,
  });
}

export function useMarkNotificationAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: notificationService.markAsRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEYS.notifications }),
  });
}
