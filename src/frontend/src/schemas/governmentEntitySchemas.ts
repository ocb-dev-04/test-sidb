import { z } from 'zod';
import {
  GOVERNMENT_ENTITY_CATEGORY_MAXIMUM_LENGTH,
  GOVERNMENT_ENTITY_NAME_MAXIMUM_LENGTH,
  GOVERNMENT_ENTITY_SECTOR_MAXIMUM_LENGTH,
  GOVERNMENT_ENTITY_STATE_BRANCH_MAXIMUM_LENGTH,
  maximumLengthMessage,
} from './validationLimits';

function requiredText(requiredMessage: string, maximumLength: number) {
  return z.string().trim().min(1, requiredMessage).max(maximumLength, maximumLengthMessage(maximumLength));
}

export const GOVERNMENT_ENTITY_SCHEMA = z.object({
  name: requiredText('El nombre es obligatorio.', GOVERNMENT_ENTITY_NAME_MAXIMUM_LENGTH),
  category: requiredText('La categoría es obligatoria.', GOVERNMENT_ENTITY_CATEGORY_MAXIMUM_LENGTH),
  stateBranch: requiredText('El poder del Estado es obligatorio.', GOVERNMENT_ENTITY_STATE_BRANCH_MAXIMUM_LENGTH),
  sector: requiredText('El sector es obligatorio.', GOVERNMENT_ENTITY_SECTOR_MAXIMUM_LENGTH),
});

export type GovernmentEntityFormValues = z.infer<typeof GOVERNMENT_ENTITY_SCHEMA>;
