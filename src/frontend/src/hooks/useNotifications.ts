import { useInfiniteQuery, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
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
  return useInfiniteQuery({
    queryKey: QUERY_KEYS.notificationList,
    queryFn: ({ pageParam }) => notificationService.list(pageParam, NOTIFICATION_PAGE_SIZE),
    initialPageParam: FIRST_NOTIFICATION_PAGE,
    getNextPageParam: (lastPage) => (lastPage.hasNextPage ? lastPage.page + 1 : undefined),
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

export function useMarkAllNotificationsAsRead() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: notificationService.markAllAsRead,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: QUERY_KEYS.notifications }),
  });
}
