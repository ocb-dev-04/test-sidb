import { describe, expect, it } from 'vitest';
import { createChangeStatusSchema, EMPTY_SERVICE_REQUEST_FORM, SERVICE_REQUEST_SCHEMA } from './serviceRequestSchemas';
import { CREATE_USER_SCHEMA } from './userSchemas';
import { SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH } from './validationLimits';

function issuesByField(result: {
  success: boolean;
  error?: { issues: { path: (string | number)[]; message: string }[] };
}) {
  return Object.fromEntries((result.error?.issues ?? []).map((issue) => [issue.path.join('.'), issue.message]));
}

describe('esquema de solicitud', () => {
  const validRequest = {
    ...EMPTY_SERVICE_REQUEST_FORM,
    title: 'Acceso a VPN',
    description: 'No puedo conectarme a la VPN institucional.',
    areaId: 1,
    requestTypeId: 2,
  };

  it('acepta una solicitud válida y convierte los identificadores de los select', () => {
    const result = SERVICE_REQUEST_SCHEMA.safeParse({ ...validRequest, areaId: '3', requestTypeId: '4' });

    expect(result.success).toBe(true);
    expect(result.data).toMatchObject({ areaId: 3, requestTypeId: 4 });
  });

  it('exige título, descripción, área y tipo con mensajes en español', () => {
    const issues = issuesByField(SERVICE_REQUEST_SCHEMA.safeParse(EMPTY_SERVICE_REQUEST_FORM));

    expect(issues).toMatchObject({
      title: 'El título es obligatorio.',
      description: 'La descripción es obligatoria.',
      areaId: 'Seleccione el área.',
      requestTypeId: 'Seleccione el tipo de solicitud.',
    });
  });

  it('limita el título al máximo del backend', () => {
    const tooLongTitle = 'a'.repeat(SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH + 1);
    const issues = issuesByField(SERVICE_REQUEST_SCHEMA.safeParse({ ...validRequest, title: tooLongTitle }));

    expect(issues.title).toBe(`Debe tener como máximo ${SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH} caracteres.`);
  });
});

describe('esquema de cambio de estado', () => {
  it('exige comentario al resolver o cerrar', () => {
    const schema = createChangeStatusSchema('InProgress');

    expect(schema.safeParse({ newStatus: 'Resolved', comment: '' }).success).toBe(false);
    expect(schema.safeParse({ newStatus: 'Resolved', comment: 'Se restableció el acceso.' }).success).toBe(true);
  });

  it('exige comentario al reabrir una solicitud cerrada', () => {
    expect(createChangeStatusSchema('Closed').safeParse({ newStatus: 'InAnalysis', comment: '  ' }).success).toBe(
      false,
    );
  });

  it('no exige comentario en transiciones intermedias', () => {
    expect(createChangeStatusSchema('Registered').safeParse({ newStatus: 'InAnalysis', comment: '' }).success).toBe(
      true,
    );
  });
});

describe('esquema de usuario', () => {
  const validUser = { fullName: 'Ana Pérez', email: 'ana@solicitudes.local', password: 'Segura2026', role: 'Analyst' };

  it('aplica la política de contraseñas del backend', () => {
    expect(CREATE_USER_SCHEMA.safeParse(validUser).success).toBe(true);
    expect(issuesByField(CREATE_USER_SCHEMA.safeParse({ ...validUser, password: 'corta1A' })).password).toMatch(
      /al menos 8/,
    );
    expect(issuesByField(CREATE_USER_SCHEMA.safeParse({ ...validUser, password: 'sinmayuscula1' })).password).toMatch(
      /mayúscula/,
    );
    expect(issuesByField(CREATE_USER_SCHEMA.safeParse({ ...validUser, password: 'SINMINUSCULA1' })).password).toMatch(
      /minúscula/,
    );
    expect(issuesByField(CREATE_USER_SCHEMA.safeParse({ ...validUser, password: 'SinDigitos' })).password).toMatch(
      /dígito/,
    );
  });
});
