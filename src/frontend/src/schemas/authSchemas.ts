import { z } from 'zod';

export const LOGIN_SCHEMA = z.object({
  email: z.string().trim().min(1, 'Ingrese su correo electrónico.').email('Ingrese un correo electrónico válido.'),
  password: z.string().min(1, 'Ingrese su contraseña.'),
});

export type LoginFormValues = z.infer<typeof LOGIN_SCHEMA>;
