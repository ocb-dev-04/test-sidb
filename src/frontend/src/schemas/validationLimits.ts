// Límites espejo de las reglas de validación del backend (FluentValidation y entidades de dominio).
export const SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH = 150;
export const SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH = 4000;
export const SERVICE_REQUEST_EVIDENCE_MAXIMUM_LENGTH = 500;
export const TRANSITION_COMMENT_MAXIMUM_LENGTH = 1000;
export const COMMENT_TEXT_MAXIMUM_LENGTH = 2000;
export const SEARCH_MAXIMUM_LENGTH = 100;
export const USER_FULL_NAME_MAXIMUM_LENGTH = 150;
export const USER_EMAIL_MAXIMUM_LENGTH = 256;
export const PASSWORD_MINIMUM_LENGTH = 8;
export const PASSWORD_MAXIMUM_LENGTH = 128;
export const CATALOG_NAME_MAXIMUM_LENGTH = 100;
export const REQUEST_TYPE_DESCRIPTION_MAXIMUM_LENGTH = 500;
export const GOVERNMENT_ENTITY_NAME_MAXIMUM_LENGTH = 250;
export const GOVERNMENT_ENTITY_CATEGORY_MAXIMUM_LENGTH = 150;
export const GOVERNMENT_ENTITY_STATE_BRANCH_MAXIMUM_LENGTH = 100;
export const GOVERNMENT_ENTITY_SECTOR_MAXIMUM_LENGTH = 150;

export function maximumLengthMessage(maximumLength: number): string {
  return `Debe tener como máximo ${maximumLength} caracteres.`;
}
