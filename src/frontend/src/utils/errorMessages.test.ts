import { describe, expect, it } from 'vitest';
import { ApiError } from '../services/apiError';
import { GENERIC_ERROR_MESSAGE, errorMessage, fieldErrors, translateErrorCode } from './errorMessages';

const HTTP_STATUS_BAD_REQUEST = 400;
const HTTP_STATUS_UNAUTHORIZED = 401;
const HTTP_STATUS_FORBIDDEN = 403;

describe('traducción de errores de la API', () => {
  it('traduce un código conocido del backend', () => {
    const error = new ApiError(HTTP_STATUS_BAD_REQUEST, {
      detail: 'A comment is required to resolve, close or reopen a service request.',
      code: 'serviceRequest.commentRequired',
    });

    expect(errorMessage(error)).toBe('Debe indicar un comentario para resolver, cerrar o reabrir la solicitud.');
  });

  it('traduce credenciales inválidas en el inicio de sesión', () => {
    const error = new ApiError(HTTP_STATUS_UNAUTHORIZED, { code: 'user.invalidCredentials' });

    expect(errorMessage(error)).toBe('El correo o la contraseña son incorrectos.');
  });

  it('usa el detail del ProblemDetails cuando el código no es conocido', () => {
    expect(translateErrorCode('something.new', 'Detalle del servidor')).toBe('Detalle del servidor');
  });

  it('usa un mensaje por estado HTTP cuando no hay código ni detalle', () => {
    expect(errorMessage(new ApiError(HTTP_STATUS_FORBIDDEN, null))).toBe(
      'No tiene permisos para realizar esta acción.',
    );
  });

  it('devuelve un mensaje genérico para errores que no vienen de la API', () => {
    expect(errorMessage(new Error('boom'))).toBe(GENERIC_ERROR_MESSAGE);
  });

  it('normaliza y traduce los errores de validación por campo', () => {
    const error = new ApiError(HTTP_STATUS_BAD_REQUEST, {
      code: 'validation.failed',
      errors: {
        Title: ["'Title' must not be empty."],
        password: ['Password must contain an uppercase letter.'],
        description: ["The length of 'Description' must be 4000 characters or fewer. You entered 4001 characters."],
      },
    });

    expect(fieldErrors(error)).toEqual({
      title: 'Este campo es obligatorio.',
      password: 'Debe contener al menos una letra mayúscula.',
      description: 'Debe tener como máximo 4000 caracteres.',
    });
  });
});
