import { lazy } from 'react';
import type { ComponentType } from 'react';
import { createBrowserRouter } from 'react-router-dom';
import type { RouteObject } from 'react-router-dom';
import { RequireAuth } from './auth/RequireAuth';
import { AppLayout } from './layout/AppLayout';
import type { RouteHandle } from './layout/routes';
import { ROUTES } from './layout/routes';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { NotFoundPage } from './pages/NotFoundPage';
import { RouteErrorPage } from './pages/RouteErrorPage';
import type { UserRole } from './types/api';

// Login e inicio van en el paquete inicial; el resto de pantallas se descarga al visitarlas.
function lazyPage<TModule>(load: () => Promise<TModule>, pick: (module: TModule) => ComponentType) {
  return lazy(async () => ({ default: pick(await load()) }));
}

const ServiceRequestListPage = lazyPage(
  () => import('./pages/ServiceRequestListPage'),
  (module) => module.ServiceRequestListPage,
);
const ServiceRequestCreatePage = lazyPage(
  () => import('./pages/ServiceRequestCreatePage'),
  (module) => module.ServiceRequestCreatePage,
);
const ServiceRequestDetailPage = lazyPage(
  () => import('./pages/ServiceRequestDetailPage'),
  (module) => module.ServiceRequestDetailPage,
);
const GovernmentEntityListPage = lazyPage(
  () => import('./pages/GovernmentEntityListPage'),
  (module) => module.GovernmentEntityListPage,
);
const GovernmentEntityFormPage = lazyPage(
  () => import('./pages/GovernmentEntityFormPage'),
  (module) => module.GovernmentEntityFormPage,
);
const UsersPage = lazyPage(
  () => import('./pages/UsersPage'),
  (module) => module.UsersPage,
);
const CatalogsPage = lazyPage(
  () => import('./pages/CatalogsPage'),
  (module) => module.CatalogsPage,
);

const ADMINISTRATOR_ONLY: readonly UserRole[] = ['Administrator'];

function titled(title: string): RouteHandle {
  return { title };
}

export const APPLICATION_ROUTES: RouteObject[] = [
  { path: ROUTES.login, element: <LoginPage />, errorElement: <RouteErrorPage standalone /> },
  {
    path: ROUTES.home,
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    errorElement: <RouteErrorPage standalone />,
    children: [
      {
        // Sin ruta propia: un error dentro de una pantalla conserva el menú y la cabecera.
        errorElement: <RouteErrorPage />,
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
