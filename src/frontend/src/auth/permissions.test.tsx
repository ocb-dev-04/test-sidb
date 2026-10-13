import { screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { CommentsSection } from '../components/CommentsSection';
import { ServiceRequestManagementPanel } from '../components/ServiceRequestManagementPanel';
import { visibleSections } from '../layout/navigation';
import { buildUser, renderWithProviders } from '../test/renderWithProviders';
import type { ServiceRequestDetailResponse } from '../types/api';
import { canEditServiceRequest } from './permissions';

vi.mock('../services/userService', () => ({
  userService: {
    assignees: vi.fn().mockResolvedValue([{ id: 'analyst-1', fullName: 'María Fernández', role: 'Analyst' }]),
  },
}));

const SERVICE_REQUEST: ServiceRequestDetailResponse = {
  id: 'request-1',
  code: 'SOL-2026-0001',
  title: 'Acceso a VPN',
  description: 'Sin acceso',
  priority: 'High',
  status: 'InAnalysis',
  areaId: 1,
  areaName: 'Tecnología',
  requestTypeId: 1,
  requestTypeName: 'Soporte técnico',
  requesterId: 'user-Requester',
  requesterName: 'Ana Rodríguez',
  createdOnUtc: '2026-10-01T12:00:00Z',
  dueDateUtc: '2026-10-02T12:00:00Z',
  isOverdue: false,
  statusHistory: [],
  comments: [],
  allowedTransitions: ['InProgress', 'WaitingForRequester', 'Resolved', 'Closed'],
  version: 'AAAAAAAAB9E=',
};

describe('visibilidad de acciones por rol', () => {
  it('oculta la gestión (estado y asignación) al solicitante', () => {
    renderWithProviders(<ServiceRequestManagementPanel serviceRequest={SERVICE_REQUEST} role="Requester" />, {
      user: buildUser('Requester'),
    });

    expect(screen.queryByText('Cambiar estado')).not.toBeInTheDocument();
    expect(screen.queryByText('Asignación')).not.toBeInTheDocument();
  });

  it('muestra al analista solo las transiciones permitidas por la API', async () => {
    renderWithProviders(<ServiceRequestManagementPanel serviceRequest={SERVICE_REQUEST} role="Analyst" />, {
      user: buildUser('Analyst'),
    });

    const statusSelect = screen.getByLabelText(/Nuevo estado/);
    const optionLabels = Array.from((statusSelect as HTMLSelectElement).options).map((option) => option.text);
    expect(optionLabels).toEqual(['En progreso', 'En espera del solicitante', 'Resuelta', 'Cerrada']);
    expect(await screen.findByRole('option', { name: /María Fernández/ })).toBeInTheDocument();
  });

  it('solo permite elegir comentario interno al personal', () => {
    const { unmount } = renderWithProviders(
      <CommentsSection serviceRequestId="request-1" comments={[]} role="Requester" />,
      { user: buildUser('Requester') },
    );
    expect(screen.queryByLabelText('Visibilidad')).not.toBeInTheDocument();
    unmount();

    renderWithProviders(<CommentsSection serviceRequestId="request-1" comments={[]} role="Analyst" />, {
      user: buildUser('Analyst'),
    });
    expect(screen.getByRole('option', { name: 'Interno' })).toBeInTheDocument();
  });

  it('muestra el menú de Administración solo al administrador', () => {
    const titles = (role: Parameters<typeof visibleSections>[0]) =>
      visibleSections(role).map((section) => section.title);

    expect(titles('Administrator')).toContain('Administración');
    expect(titles('Analyst')).not.toContain('Administración');
    expect(titles('Requester')).not.toContain('Administración');
  });

  it('permite al solicitante editar solo su solicitud mientras está Registrada', () => {
    const ownRegistered = { requesterId: 'owner', status: 'Registered' } as const;

    expect(canEditServiceRequest('Requester', 'owner', ownRegistered)).toBe(true);
    expect(canEditServiceRequest('Requester', 'owner', { ...ownRegistered, status: 'InAnalysis' })).toBe(false);
    expect(canEditServiceRequest('Requester', 'someone-else', ownRegistered)).toBe(false);
    expect(canEditServiceRequest('Analyst', 'analyst', { ...ownRegistered, status: 'InProgress' })).toBe(true);
    expect(canEditServiceRequest('Administrator', 'admin', { ...ownRegistered, status: 'Closed' })).toBe(false);
  });
});
