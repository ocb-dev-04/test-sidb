import { describe, expect, it } from 'vitest';
import { SERVICE_REQUEST_PRIORITIES, SERVICE_REQUEST_STATUSES } from '../types/api';
import { dataSourceLabel, priorityLabel, roleLabel, statusLabel, visibilityLabel } from './labels';

describe('etiquetas en español', () => {
  it.each([
    ['Registered', 'Registrada'],
    ['InAnalysis', 'En análisis'],
    ['InProgress', 'En progreso'],
    ['WaitingForRequester', 'En espera del solicitante'],
    ['Resolved', 'Resuelta'],
    ['Closed', 'Cerrada'],
  ] as const)('traduce el estado %s como "%s"', (status, expected) => {
    expect(statusLabel(status)).toBe(expected);
  });

  it.each([
    ['Low', 'Baja'],
    ['Medium', 'Media'],
    ['High', 'Alta'],
    ['Critical', 'Crítica'],
  ] as const)('traduce la prioridad %s como "%s"', (priority, expected) => {
    expect(priorityLabel(priority)).toBe(expected);
  });

  it('cubre todos los estados y prioridades del contrato sin dejar valores en inglés', () => {
    SERVICE_REQUEST_STATUSES.forEach((status) => expect(statusLabel(status)).not.toBe(status));
    SERVICE_REQUEST_PRIORITIES.forEach((priority) => expect(priorityLabel(priority)).not.toBe(priority));
  });

  it('traduce roles, visibilidad de comentarios y fuentes de datos', () => {
    expect(roleLabel('Administrator')).toBe('Administrador');
    expect(roleLabel('Analyst')).toBe('Analista');
    expect(roleLabel('Requester')).toBe('Solicitante');
    expect(visibilityLabel('Internal')).toBe('Interno');
    expect(visibilityLabel('Public')).toBe('Público');
    expect(dataSourceLabel('TextFile')).toBe('Archivo de texto');
    expect(dataSourceLabel('Excel')).toBe('Excel (solo lectura)');
  });
});
