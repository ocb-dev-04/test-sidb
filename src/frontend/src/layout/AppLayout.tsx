import { useEffect, useRef, useState } from 'react';
import { Outlet, useMatches } from 'react-router-dom';
import { LogoutIcon, MenuIcon } from '../components/Icons';
import { NotificationBell } from '../components/NotificationBell';
import { useAuth } from '../hooks/useAuth';
import { roleLabel } from '../utils/labels';
import type { RouteHandle } from './routes';
import { SIDEBAR_ID, Sidebar } from './Sidebar';

const DEFAULT_PAGE_TITLE = 'Gestión de solicitudes';

function usePageTitle(): string {
  const matches = useMatches();
  const titledMatch = [...matches].reverse().find((match) => (match.handle as RouteHandle | undefined)?.title);
  return (titledMatch?.handle as RouteHandle | undefined)?.title ?? DEFAULT_PAGE_TITLE;
}

export function AppLayout() {
  const { user, logout } = useAuth();
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const menuToggleReference = useRef<HTMLButtonElement>(null);
  const pageTitle = usePageTitle();

  // En pantallas angostas el menú es un panel: el foco entra al abrirlo y Escape lo cierra devolviendo el foco.
  useEffect(() => {
    if (!isSidebarOpen) {
      return;
    }
    document.getElementById(SIDEBAR_ID)?.querySelector<HTMLElement>('a[href]')?.focus();
    const closeOnEscape = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        setIsSidebarOpen(false);
        menuToggleReference.current?.focus();
      }
    };
    document.addEventListener('keydown', closeOnEscape);
    return () => document.removeEventListener('keydown', closeOnEscape);
  }, [isSidebarOpen]);

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
            ref={menuToggleReference}
            type="button"
            className="icon-button menu-toggle"
            aria-label="Abrir menú"
            aria-controls={SIDEBAR_ID}
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
              onClick={() => logout()}
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
