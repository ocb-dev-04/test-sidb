import { createBrowserRouter } from 'react-router-dom';
import type { RouteObject } from 'react-router-dom';
import { RequireAuth } from './auth/RequireAuth';
import { AppLayout } from './layout/AppLayout';
import type { RouteHandle } from './layout/routes';
import { ROUTES } from './layout/routes';
import { CatalogsPage } from './pages/CatalogsPage';
import { DashboardPage } from './pages/DashboardPage';
import { GovernmentEntityFormPage } from './pages/GovernmentEntityFormPage';
import { GovernmentEntityListPage } from './pages/GovernmentEntityListPage';
import { LoginPage } from './pages/LoginPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { ServiceRequestCreatePage } from './pages/ServiceRequestCreatePage';
import { ServiceRequestDetailPage } from './pages/ServiceRequestDetailPage';
import { ServiceRequestListPage } from './pages/ServiceRequestListPage';
import { UsersPage } from './pages/UsersPage';
import type { UserRole } from './types/api';

const ADMINISTRATOR_ONLY: readonly UserRole[] = ['Administrator'];

function titled(title: string): RouteHandle {
  return { title };
}

export const APPLICATION_ROUTES: RouteObject[] = [
  { path: ROUTES.login, element: <LoginPage /> },
  {
    path: ROUTES.home,
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <DashboardPage />, handle: titled('Inicio') },
      { path: 'solicitudes', element: <ServiceRequestListPage />, handle: titled('Consulta de solicitudes') },
      { path: 'solicitudes/nueva', element: <ServiceRequestCreatePage />, handle: titled('Crear solicitud') },
      { path: 'solicitudes/:id', element: <ServiceRequestDetailPage />, handle: titled('Detalle de solicitud') },
      {
        path: 'entidades-gubernamentales',
        element: <GovernmentEntityListPage />,
        handle: titled('Entidades gubernamentales'),
      },
      {
        path: 'entidades-gubernamentales/nueva',
        element: (
          <RequireAuth roles={ADMINISTRATOR_ONLY}>
            <GovernmentEntityFormPage />
          </RequireAuth>
        ),
        handle: titled('Crear entidad gubernamental'),
      },
      {
        path: 'entidades-gubernamentales/:id/editar',
        element: (
          <RequireAuth roles={ADMINISTRATOR_ONLY}>
            <GovernmentEntityFormPage />
          </RequireAuth>
        ),
        handle: titled('Editar entidad gubernamental'),
      },
      {
        path: 'administracion/usuarios',
        element: (
          <RequireAuth roles={ADMINISTRATOR_ONLY}>
            <UsersPage />
          </RequireAuth>
        ),
        handle: titled('Usuarios'),
      },
      {
        path: 'administracion/catalogos',
        element: (
          <RequireAuth roles={ADMINISTRATOR_ONLY}>
            <CatalogsPage />
          </RequireAuth>
        ),
        handle: titled('Catálogos'),
      },
      { path: '*', element: <NotFoundPage />, handle: titled('Página no encontrada') },
    ],
  },
];

export function createApplicationRouter() {
  return createBrowserRouter(APPLICATION_ROUTES, {
    future: {
      v7_relativeSplatPath: true,
      v7_fetcherPersist: true,
      v7_normalizeFormMethod: true,
      v7_partialHydration: true,
      v7_skipActionErrorRevalidation: true,
    },
  });
}
