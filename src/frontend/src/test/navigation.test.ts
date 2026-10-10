import { describe, expect, it } from 'vitest';
import {
  isNavigationItemActive,
  NAVIGATION_SECTIONS,
  type NavigationItem,
  type NavigationSection,
} from '../layout/navigation';
import { ROUTES } from '../layout/routes';

const SERVICE_REQUESTS_TITLE = 'Solicitudes';
const GOVERNMENT_ENTITIES_TITLE = 'Entidades gubernamentales';
const LIST_LABEL = 'Consulta';
const CREATE_LABEL = 'Crear registro';
const SAMPLE_ENTITY_ID = 7;

function findSection(title: string): NavigationSection {
  const section = NAVIGATION_SECTIONS.find((candidate) => candidate.title === title);
  if (!section) {
    throw new Error(`Sección no encontrada: ${title}`);
  }
  return section;
}

function findItem(section: NavigationSection, label: string): NavigationItem {
  const item = section.items.find((candidate) => candidate.label === label);
  if (!item) {
    throw new Error(`Opción no encontrada: ${label}`);
  }
  return item;
}

describe('isNavigationItemActive', () => {
  const serviceRequests = findSection(SERVICE_REQUESTS_TITLE);
  const governmentEntities = findSection(GOVERNMENT_ENTITIES_TITLE);

  it('resalta "Consulta" en el detalle de una solicitud', () => {
    const listItem = findItem(serviceRequests, LIST_LABEL);

    expect(isNavigationItemActive(listItem, serviceRequests, ROUTES.serviceRequestDetail('abc'))).toBe(true);
  });

  it('no resalta "Consulta" cuando se está creando un registro', () => {
    const listItem = findItem(serviceRequests, LIST_LABEL);
    const createItem = findItem(serviceRequests, CREATE_LABEL);

    expect(isNavigationItemActive(listItem, serviceRequests, ROUTES.newServiceRequest)).toBe(false);
    expect(isNavigationItemActive(createItem, serviceRequests, ROUTES.newServiceRequest)).toBe(true);
  });

  it('resalta "Consulta" al editar una entidad gubernamental', () => {
    const listItem = findItem(governmentEntities, LIST_LABEL);

    expect(isNavigationItemActive(listItem, governmentEntities, ROUTES.editGovernmentEntity(SAMPLE_ENTITY_ID))).toBe(
      true,
    );
  });
});
