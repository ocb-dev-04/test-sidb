import { isApiError } from '../services/apiError';

const HTTP_STATUS_FORBIDDEN = 403;
const HTTP_STATUS_NOT_FOUND = 404;
const HTTP_STATUS_TOO_MANY_REQUESTS = 429;
const HTTP_STATUS_SERVER_ERROR = 500;

export const GENERIC_ERROR_MESSAGE = 'Ocurrió un error inesperado. Intente de nuevo.';

export const ERROR_CODE_MESSAGES: Record<string, string> = {
  'network.unavailable': 'No fue posible conectar con el servidor. Verifique su conexión e intente de nuevo.',
  'validation.failed': 'Revise los datos ingresados: hay campos con errores.',
  'user.notFound': 'El usuario no existe.',
  'user.emailAlreadyInUse': 'El correo electrónico ya está registrado.',
  'user.invalidCredentials': 'El correo o la contraseña son incorrectos.',
  'user.inactive': 'El usuario está inactivo. Contacte al administrador.',
  'user.cannotDeactivateSelf': 'Un administrador no puede desactivarse ni quitarse el rol a sí mismo.',
  'area.notFound': 'El área no existe.',
  'area.inactive': 'El área seleccionada está inactiva.',
  'area.nameInUse': 'Ya existe un área con ese nombre.',
  'requestType.notFound': 'El tipo de solicitud no existe.',
  'requestType.inactive': 'El tipo de solicitud seleccionado está inactivo.',
  'requestType.nameInUse': 'Ya existe un tipo de solicitud con ese nombre.',
  'serviceRequest.notFound': 'La solicitud no existe o no tiene acceso a ella.',
  'serviceRequest.closed': 'Una solicitud cerrada no puede modificarse.',
  'serviceRequest.sameStatus': 'La solicitud ya tiene ese estado.',
  'serviceRequest.commentRequired': 'Debe indicar un comentario para resolver, cerrar o reabrir la solicitud.',
  'serviceRequest.reopenNotAllowed': 'Solo administradores y analistas pueden reabrir una solicitud cerrada.',
  'serviceRequest.alreadyAssigned': 'La solicitud ya está asignada a ese usuario.',
  'serviceRequest.invalidAssignee': 'El responsable debe ser un administrador o analista activo.',
  'serviceRequest.editNotAllowed': 'Solo puede editar sus propias solicitudes mientras estén en estado Registrada.',
  'serviceRequest.internalCommentNotAllowed': 'Los solicitantes no pueden agregar comentarios internos.',
  'serviceRequest.managementRequired': 'Solo administradores y analistas pueden gestionar solicitudes.',
  'serviceRequest.concurrencyConflict': 'Otro usuario modificó la solicitud. Recargue la página e intente de nuevo.',
  'serviceRequest.invalidTransition': 'La transición de estado solicitada no está permitida.',
  'notification.notFound': 'La notificación no existe.',
  'governmentEntity.notFound': 'La entidad gubernamental no existe.',
  'governmentEntity.nameInUse': 'Ya existe una entidad gubernamental con ese nombre.',
  'governmentEntity.sourceReadOnly': 'La fuente de datos seleccionada es de solo lectura.',
  'governmentEntity.sourceNotSupported': 'La fuente de datos seleccionada no está soportada.',
  'governmentEntity.sameSourceAndDestination': 'La fuente de origen y la de destino deben ser distintas.',
};

const STATUS_FALLBACK_MESSAGES: Record<number, string> = {
  [HTTP_STATUS_FORBIDDEN]: 'No tiene permisos para realizar esta acción.',
  [HTTP_STATUS_NOT_FOUND]: 'El recurso solicitado no existe.',
  [HTTP_STATUS_TOO_MANY_REQUESTS]: 'Demasiados intentos. Espere un momento e intente de nuevo.',
  [HTTP_STATUS_SERVER_ERROR]: 'Ocurrió un error inesperado en el servidor. Intente más tarde.',
};

export function translateErrorCode(code: string | undefined, fallback?: string): string {
  const translation = code ? ERROR_CODE_MESSAGES[code] : undefined;
  return translation ?? (fallback || GENERIC_ERROR_MESSAGE);
}

export function errorMessage(error: unknown): string {
  if (!isApiError(error)) {
    return GENERIC_ERROR_MESSAGE;
  }
  return translateErrorCode(error.code, error.detail ?? STATUS_FALLBACK_MESSAGES[error.status]);
}

// La API devuelve los mensajes de FluentValidation en inglés; se traducen los patrones conocidos.
const VALIDATION_MESSAGE_TRANSLATIONS: ReadonlyArray<readonly [RegExp, (match: RegExpMatchArray) => string]> = [
  [/must not be empty/i, () => 'Este campo es obligatorio.'],
  [/is not a valid email/i, () => 'El correo electrónico no es válido.'],
  [/must be (\d+) characters or fewer/i, (match) => `Debe tener como máximo ${match[1]} caracteres.`],
  [/must be at least (\d+) characters/i, (match) => `Debe tener al menos ${match[1]} caracteres.`],
  [/uppercase letter/i, () => 'Debe contener al menos una letra mayúscula.'],
  [/lowercase letter/i, () => 'Debe contener al menos una letra minúscula.'],
  [/contain a digit/i, () => 'Debe contener al menos un dígito.'],
  [/cannot be blank/i, () => 'No puede quedar en blanco.'],
  [/range of values|is not valid for/i, () => 'El valor seleccionado no es válido.'],
  [/must be greater than/i, () => 'Seleccione un valor válido.'],
];

export function translateValidationMessage(message: string): string {
  for (const [pattern, translate] of VALIDATION_MESSAGE_TRANSLATIONS) {
    const match = message.match(pattern);
    if (match) {
      return translate(match);
    }
  }
  return message;
}

export function fieldErrors(error: unknown): Record<string, string> {
  if (!isApiError(error)) {
    return {};
  }
  return Object.fromEntries(
    Object.entries(error.fieldErrors).map(([field, messages]) => [
      field.charAt(0).toLowerCase() + field.slice(1),
      translateValidationMessage(messages[0] ?? ''),
    ]),
  );
}
