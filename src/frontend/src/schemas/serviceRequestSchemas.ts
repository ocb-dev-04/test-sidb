import { z } from 'zod';
import { transitionRequiresComment } from '../auth/permissions';
import { COMMENT_VISIBILITIES, SERVICE_REQUEST_PRIORITIES, SERVICE_REQUEST_STATUSES } from '../types/api';
import type { ServiceRequestStatus } from '../types/api';
import {
  COMMENT_TEXT_MAXIMUM_LENGTH,
  SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH,
  SERVICE_REQUEST_EVIDENCE_MAXIMUM_LENGTH,
  SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH,
  TRANSITION_COMMENT_MAXIMUM_LENGTH,
  maximumLengthMessage,
} from './validationLimits';

export const SERVICE_REQUEST_SCHEMA = z.object({
  title: z
    .string()
    .trim()
    .min(1, 'El título es obligatorio.')
    .max(SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH, maximumLengthMessage(SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH)),
  description: z
    .string()
    .trim()
    .min(1, 'La descripción es obligatoria.')
    .max(SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH, maximumLengthMessage(SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH)),
  priority: z.enum(SERVICE_REQUEST_PRIORITIES, { errorMap: () => ({ message: 'Seleccione la prioridad.' }) }),
  areaId: z.coerce.number().int().positive('Seleccione el área.'),
  requestTypeId: z.coerce.number().int().positive('Seleccione el tipo de solicitud.'),
  evidenceReference: z
    .string()
    .trim()
    .max(SERVICE_REQUEST_EVIDENCE_MAXIMUM_LENGTH, maximumLengthMessage(SERVICE_REQUEST_EVIDENCE_MAXIMUM_LENGTH)),
});

export type ServiceRequestFormValues = z.infer<typeof SERVICE_REQUEST_SCHEMA>;

export const EMPTY_SERVICE_REQUEST_FORM: ServiceRequestFormValues = {
  title: '',
  description: '',
  priority: 'Medium',
  areaId: 0,
  requestTypeId: 0,
  evidenceReference: '',
};

export const COMMENT_SCHEMA = z.object({
  text: z
    .string()
    .trim()
    .min(1, 'Escriba el comentario.')
    .max(COMMENT_TEXT_MAXIMUM_LENGTH, maximumLengthMessage(COMMENT_TEXT_MAXIMUM_LENGTH)),
  visibility: z.enum(COMMENT_VISIBILITIES),
});

export type CommentFormValues = z.infer<typeof COMMENT_SCHEMA>;

export function createChangeStatusSchema(currentStatus: ServiceRequestStatus) {
  return z
    .object({
      newStatus: z.enum(SERVICE_REQUEST_STATUSES, { errorMap: () => ({ message: 'Seleccione el nuevo estado.' }) }),
      comment: z
        .string()
        .trim()
        .max(TRANSITION_COMMENT_MAXIMUM_LENGTH, maximumLengthMessage(TRANSITION_COMMENT_MAXIMUM_LENGTH)),
    })
    .superRefine((values, context) => {
      if (transitionRequiresComment(currentStatus, values.newStatus) && values.comment.length === 0) {
        context.addIssue({
          code: z.ZodIssueCode.custom,
          path: ['comment'],
          message: 'El comentario es obligatorio para resolver, cerrar o reabrir la solicitud.',
        });
      }
    });
}

export type ChangeStatusFormValues = z.infer<ReturnType<typeof createChangeStatusSchema>>;
