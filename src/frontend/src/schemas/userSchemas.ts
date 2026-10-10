import { z } from 'zod';
import { USER_ROLES } from '../types/api';
import {
  PASSWORD_MAXIMUM_LENGTH,
  PASSWORD_MINIMUM_LENGTH,
  USER_EMAIL_MAXIMUM_LENGTH,
  USER_FULL_NAME_MAXIMUM_LENGTH,
  maximumLengthMessage,
} from './validationLimits';

const FULL_NAME_SCHEMA = z
  .string()
  .trim()
  .min(1, 'El nombre completo es obligatorio.')
  .max(USER_FULL_NAME_MAXIMUM_LENGTH, maximumLengthMessage(USER_FULL_NAME_MAXIMUM_LENGTH));

const ROLE_SCHEMA = z.enum(USER_ROLES, { errorMap: () => ({ message: 'Seleccione el rol.' }) });

export const PASSWORD_SCHEMA = z
  .string()
  .min(PASSWORD_MINIMUM_LENGTH, `La contraseña debe tener al menos ${PASSWORD_MINIMUM_LENGTH} caracteres.`)
  .max(PASSWORD_MAXIMUM_LENGTH, maximumLengthMessage(PASSWORD_MAXIMUM_LENGTH))
  .regex(/[A-Z]/, 'La contraseña debe contener al menos una letra mayúscula.')
  .regex(/[a-z]/, 'La contraseña debe contener al menos una letra minúscula.')
  .regex(/\d/, 'La contraseña debe contener al menos un dígito.');

export const CREATE_USER_SCHEMA = z.object({
  fullName: FULL_NAME_SCHEMA,
  email: z
    .string()
    .trim()
    .min(1, 'El correo electrónico es obligatorio.')
    .email('Ingrese un correo electrónico válido.')
    .max(USER_EMAIL_MAXIMUM_LENGTH, maximumLengthMessage(USER_EMAIL_MAXIMUM_LENGTH)),
  password: PASSWORD_SCHEMA,
  role: ROLE_SCHEMA,
});

export type CreateUserFormValues = z.infer<typeof CREATE_USER_SCHEMA>;

export const UPDATE_USER_SCHEMA = z.object({
  fullName: FULL_NAME_SCHEMA,
  role: ROLE_SCHEMA,
  isActive: z.boolean(),
});

export type UpdateUserFormValues = z.infer<typeof UPDATE_USER_SCHEMA>;
