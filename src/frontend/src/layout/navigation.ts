import type { UserRole } from '../types/api';
import { ROUTES } from './routes';

export interface NavigationItem {
  label: string;
  to: string;
  isHome?: boolean;
  roles?: readonly UserRole[];
}

export interface NavigationSection {
  title?: string;
  items: NavigationItem[];
  roles?: readonly UserRole[];
}

export const NAVIGATION_SECTIONS: readonly NavigationSection[] = [
  { items: [{ label: 'Inicio', to: ROUTES.home, isHome: true }] },
  {
    title: 'Solicitudes',
    items: [
      { label: 'Consulta', to: ROUTES.serviceRequests },
      { label: 'Crear registro', to: ROUTES.newServiceRequest },
    ],
  },
  {
    title: 'Entidades gubernamentales',
    items: [
      { label: 'Consulta', to: ROUTES.governmentEntities },
      { label: 'Crear registro', to: ROUTES.newGovernmentEntity, roles: ['Administrator'] },
    ],
  },
  {
    title: 'Administración',
    roles: ['Administrator'],
    items: [
      { label: 'Usuarios', to: ROUTES.users },
      { label: 'Catálogos', to: ROUTES.catalogs },
    ],
  },
];

function isAllowed(roles: readonly UserRole[] | undefined, role: UserRole | undefined): boolean {
  return !roles || (role !== undefined && roles.includes(role));
}

// Las rutas hijas (detalle, edición) mantienen resaltada la opción "Consulta" de su sección.
export function isNavigationItemActive(item: NavigationItem, section: NavigationSection, pathname: string): boolean {
  if (pathname === item.to) {
    return true;
  }

  const isDescendant = item.to !== ROUTES.home && pathname.startsWith(`${item.to}/`);
  const siblingMatches = section.items.some(
    (sibling) => sibling.to !== item.to && (pathname === sibling.to || pathname.startsWith(`${sibling.to}/`)),
  );
  return isDescendant && !siblingMatches;
}

export function visibleSections(role: UserRole | undefined): NavigationSection[] {
  return NAVIGATION_SECTIONS.filter((section) => isAllowed(section.roles, role)).map((section) => ({
    ...section,
    items: section.items.filter((item) => isAllowed(item.roles, role)),
  }));
}
