import { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  useMarkAllNotificationsAsRead,
  useMarkNotificationAsRead,
  useNotificationList,
  useUnreadNotificationCount,
} from '../hooks/useNotifications';
import { ROUTES } from '../layout/routes';
import { errorMessage } from '../utils/errorMessages';
import { formatDateTime } from '../utils/formatting';
import { NOTIFICATION_TYPE_LABELS } from '../utils/labels';
import { BellIcon } from './Icons';
import { EmptyState, LoadingState } from './States';

const MAXIMUM_DISPLAYED_COUNT = 99;

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const containerReference = useRef<HTMLDivElement>(null);
  const unreadCount = useUnreadNotificationCount();
  const notifications = useNotificationList(isOpen);
  const markAsRead = useMarkNotificationAsRead();
  const markAllAsRead = useMarkAllNotificationsAsRead();
  const count = unreadCount.data ?? 0;
  const items = notifications.data?.pages.flatMap((page) => page.items) ?? [];

  useEffect(() => {
    if (!isOpen) {
      return;
    }
    const closeOnOutsideClick = (event: MouseEvent) => {
      if (!containerReference.current?.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', closeOnOutsideClick);
    document.addEventListener('keydown', closeOnEscape);
    return () => {
      document.removeEventListener('mousedown', closeOnOutsideClick);
      document.removeEventListener('keydown', closeOnEscape);
    };
  }, [isOpen]);

  return (
    <div className="header-relative" ref={containerReference}>
      <button
        type="button"
        className="icon-button"
        aria-label={`Notificaciones (${count} sin leer)`}
        aria-expanded={isOpen}
        onClick={() => setIsOpen((current) => !current)}
      >
        <BellIcon />
        {count > 0 && (
          <span className="counter-badge">
            {count > MAXIMUM_DISPLAYED_COUNT ? `${MAXIMUM_DISPLAYED_COUNT}+` : count}
          </span>
        )}
      </button>
      {isOpen && (
        <div className="popover" role="dialog" aria-label="Notificaciones">
          <div className="popover__header">
            <span>Notificaciones</span>
            <span className="inline">
              <span className="table__muted">{count} sin leer</span>
              {count > 0 && (
                <button
                  type="button"
                  className="button button--ghost button--small"
                  disabled={markAllAsRead.isPending}
                  onClick={() => markAllAsRead.mutate()}
                >
                  Marcar todas como leídas
                </button>
              )}
            </span>
          </div>
          {notifications.isPending && <LoadingState />}
          {notifications.isError && <p className="state text-danger">{errorMessage(notifications.error)}</p>}
          {notifications.data && items.length === 0 && <EmptyState title="No tiene notificaciones" />}
          {items.length > 0 && (
            <ul className="popover__list">
              {items.map((notification) => {
                const isUnread = !notification.readOnUtc;
                return (
                  <li key={notification.id} className={`notification ${isUnread ? 'notification--unread' : ''}`.trim()}>
                    <span className="notification__subject">{notification.subject}</span>
                    <span className="notification__message">{notification.message}</span>
                    <div className="notification__footer">
                      <span>
                        {NOTIFICATION_TYPE_LABELS[notification.type] ?? notification.type} ·{' '}
                        {formatDateTime(notification.createdOnUtc)}
                      </span>
                      <span className="inline">
                        {notification.serviceRequestId && (
                          <Link
                            to={ROUTES.serviceRequestDetail(notification.serviceRequestId)}
                            onClick={() => {
                              if (isUnread) {
                                markAsRead.mutate(notification.id);
                              }
                              setIsOpen(false);
                            }}
                          >
                            Ver solicitud
                          </Link>
                        )}
                        {isUnread && (
                          <button
                            type="button"
                            className="button button--ghost button--small"
                            disabled={markAsRead.isPending}
                            onClick={() => markAsRead.mutate(notification.id)}
                          >
                            Marcar como leída
                          </button>
                        )}
                      </span>
                    </div>
                  </li>
                );
              })}
            </ul>
          )}
          {notifications.hasNextPage && (
            <div className="popover__footer">
              <button
                type="button"
                className="button button--ghost button--small"
                disabled={notifications.isFetchingNextPage}
                onClick={() => notifications.fetchNextPage()}
              >
                {notifications.isFetchingNextPage ? 'Cargando…' : 'Ver más'}
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
