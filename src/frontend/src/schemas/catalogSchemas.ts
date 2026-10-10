import { z } from 'zod';
import {
  CATALOG_NAME_MAXIMUM_LENGTH,
  REQUEST_TYPE_DESCRIPTION_MAXIMUM_LENGTH,
  maximumLengthMessage,
} from './validationLimits';

export const CATALOG_ITEM_SCHEMA = z.object({
  name: z
    .string()
    .trim()
    .min(1, 'El nombre es obligatorio.')
    .max(CATALOG_NAME_MAXIMUM_LENGTH, maximumLengthMessage(CATALOG_NAME_MAXIMUM_LENGTH)),
  description: z
    .string()
    .trim()
    .max(REQUEST_TYPE_DESCRIPTION_MAXIMUM_LENGTH, maximumLengthMessage(REQUEST_TYPE_DESCRIPTION_MAXIMUM_LENGTH)),
  isActive: z.boolean(),
});

export type CatalogItemFormValues = z.infer<typeof CATALOG_ITEM_SCHEMA>;
