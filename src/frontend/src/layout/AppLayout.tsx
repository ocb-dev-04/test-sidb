import { useEffect, useState } from 'react';
import { Outlet, useMatches } from 'react-router-dom';
import { LogoutIcon, MenuIcon } from '../components/Icons';
import { NotificationBell } from '../components/NotificationBell';
import { useAuth } from '../hooks/useAuth';
import { roleLabel } from '../utils/labels';
import type { RouteHandle } from './routes';
import { Sidebar } from './Sidebar';

const DEFAULT_PAGE_TITLE = 'Gestión de solicitudes';

function usePageTitle(): string {
  const matches = useMatches();
  const titledMatch = [...matches].reverse().find((match) => (match.handle as RouteHandle | undefined)?.title);
  return (titledMatch?.handle as RouteHandle | undefined)?.title ?? DEFAULT_PAGE_TITLE;
}

export function AppLayout() {
  const { user, logout } = useAuth();
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const pageTitle = usePageTitle();

  useEffect(() => {
    document.title = `${pageTitle} | Superintendencia de Bancos`;
  }, [pageTitle]);

  return (
    <div className="app-shell">
      <Sidebar role={user?.role} isOpen={isSidebarOpen} onNavigate={() => setIsSidebarOpen(false)} />
      <div
        className={`sidebar-backdrop ${isSidebarOpen ? 'sidebar-backdrop--visible' : ''}`.trim()}
        onClick={() => setIsSidebarOpen(false)}
        aria-hidden="true"
      />
      <div className="main-column">
        <header className="top-header">
          <button
            type="button"
            className="icon-button menu-toggle"
            aria-label="Abrir menú"
            aria-controls="main-sidebar"
            aria-expanded={isSidebarOpen}
            onClick={() => setIsSidebarOpen((current) => !current)}
          >
            <MenuIcon />
          </button>
          <h1 className="top-header__title">{pageTitle}</h1>
          <div className="top-header__actions">
            <NotificationBell />
            {user && (
              <div className="user-chip">
                <span className="user-chip__name">{user.fullName}</span>
                <span className="user-chip__role">{roleLabel(user.role)}</span>
              </div>
            )}
            <button
              type="button"
              className="icon-button"
              aria-label="Cerrar sesión"
              title="Cerrar sesión"
              onClick={logout}
            >
              <LogoutIcon />
            </button>
          </div>
        </header>
        <main className="content-area">
          <div className="content-stack">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
