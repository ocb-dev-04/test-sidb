import type { FieldValues, Path, UseFormSetError } from 'react-hook-form';
import { fieldErrors } from './errorMessages';

// Muestra junto a cada campo los errores de validación que devuelve la API (ProblemDetails.errors).
export function applyServerFieldErrors<TValues extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<TValues>,
  knownFields: readonly Path<TValues>[],
): boolean {
  let appliedAny = false;
  Object.entries(fieldErrors(error)).forEach(([field, message]) => {
    const knownField = knownFields.find((candidate) => candidate === field);
    if (knownField) {
      setError(knownField, { type: 'server', message });
      appliedAny = true;
    }
  });
  return appliedAny;
}
