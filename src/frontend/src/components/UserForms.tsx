import { zodResolver } from '@hookform/resolvers/zod';
import { forwardRef, useState } from 'react';
import type { SelectHTMLAttributes } from 'react';
import { useForm } from 'react-hook-form';
import { useUnsavedChangesGuard } from '../hooks/useUnsavedChangesGuard';
import { CREATE_USER_SCHEMA, UPDATE_USER_SCHEMA } from '../schemas/userSchemas';
import type { CreateUserFormValues, UpdateUserFormValues } from '../schemas/userSchemas';
import {
  PASSWORD_MAXIMUM_LENGTH,
  PASSWORD_MINIMUM_LENGTH,
  USER_EMAIL_MAXIMUM_LENGTH,
  USER_FULL_NAME_MAXIMUM_LENGTH,
} from '../schemas/validationLimits';
import { USER_ROLES } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { applyServerFieldErrors } from '../utils/formErrors';
import { ROLE_LABELS } from '../utils/labels';
import { FormField } from './FormField';
import { Alert } from './States';

const CREATE_FIELDS = ['fullName', 'email', 'password', 'role'] as const;
const UPDATE_FIELDS = ['fullName', 'role', 'isActive'] as const;

interface FormActionsProps {
  isSubmitting: boolean;
  submitLabel: string;
  onCancel: () => void;
}

function FormActions({ isSubmitting, submitLabel, onCancel }: FormActionsProps) {
  return (
    <div className="form-actions">
      <button type="button" className="button button--secondary" onClick={onCancel} disabled={isSubmitting}>
        Cancelar
      </button>
      <button type="submit" className="button button--primary" disabled={isSubmitting}>
        {isSubmitting ? 'Guardando…' : submitLabel}
      </button>
    </div>
  );
}

// react-hook-form necesita el ref del <select> para leer y validar el rol elegido.
const RoleSelect = forwardRef<HTMLSelectElement, SelectHTMLAttributes<HTMLSelectElement>>(
  function RoleSelect(properties, reference) {
    return (
      <select className="select" ref={reference} {...properties}>
        {USER_ROLES.map((role) => (
          <option key={role} value={role}>
            {ROLE_LABELS[role]}
          </option>
        ))}
      </select>
    );
  },
);

interface CreateUserFormProps {
  onSubmit: (values: CreateUserFormValues) => Promise<void>;
  onCancel: () => void;
}

export function CreateUserForm({ onSubmit, onCancel }: CreateUserFormProps) {
  const [submitError, setSubmitError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(CREATE_USER_SCHEMA),
    defaultValues: { fullName: '', email: '', password: '', role: 'Requester' },
  });
  const { cancel } = useUnsavedChangesGuard(isDirty, isSubmitting, onCancel);

  const submit = async (values: CreateUserFormValues) => {
    setSubmitError(null);
    try {
      await onSubmit(values);
    } catch (error) {
      applyServerFieldErrors(error, setError, CREATE_FIELDS);
      setSubmitError(errorMessage(error));
    }
  };

  return (
    <form className="stack" onSubmit={handleSubmit(submit)} noValidate>
      {submitError && <Alert tone="error">{submitError}</Alert>}
      <FormField label="Nombre completo" required error={errors.fullName?.message}>
        <input className="input" maxLength={USER_FULL_NAME_MAXIMUM_LENGTH} {...register('fullName')} />
      </FormField>
      <FormField label="Correo electrónico" required error={errors.email?.message}>
        <input className="input" type="email" maxLength={USER_EMAIL_MAXIMUM_LENGTH} {...register('email')} />
      </FormField>
      <FormField
        label="Contraseña"
        required
        error={errors.password?.message}
        hint={`Mínimo ${PASSWORD_MINIMUM_LENGTH} caracteres, con mayúscula, minúscula y dígito.`}
      >
        <input
          className="input"
          type="password"
          autoComplete="new-password"
          maxLength={PASSWORD_MAXIMUM_LENGTH}
          {...register('password')}
        />
      </FormField>
      <FormField label="Rol" required error={errors.role?.message}>
        <RoleSelect {...register('role')} />
      </FormField>
      <FormActions isSubmitting={isSubmitting} submitLabel="Crear usuario" onCancel={cancel} />
    </form>
  );
}

interface UpdateUserFormProps {
  defaultValues: UpdateUserFormValues;
  onSubmit: (values: UpdateUserFormValues) => Promise<void>;
  onCancel: () => void;
}

export function UpdateUserForm({ defaultValues, onSubmit, onCancel }: UpdateUserFormProps) {
  const [submitError, setSubmitError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<UpdateUserFormValues>({ resolver: zodResolver(UPDATE_USER_SCHEMA), defaultValues });
  const { cancel } = useUnsavedChangesGuard(isDirty, isSubmitting, onCancel);

  const submit = async (values: UpdateUserFormValues) => {
    setSubmitError(null);
    try {
      await onSubmit(values);
    } catch (error) {
      applyServerFieldErrors(error, setError, UPDATE_FIELDS);
      setSubmitError(errorMessage(error));
    }
  };

  return (
    <form className="stack" onSubmit={handleSubmit(submit)} noValidate>
      {submitError && <Alert tone="error">{submitError}</Alert>}
      <FormField label="Nombre completo" required error={errors.fullName?.message}>
        <input className="input" maxLength={USER_FULL_NAME_MAXIMUM_LENGTH} {...register('fullName')} />
      </FormField>
      <FormField label="Rol" required error={errors.role?.message}>
        <RoleSelect {...register('role')} />
      </FormField>
      <label className="checkbox">
        <input type="checkbox" {...register('isActive')} />
        Usuario activo
      </label>
      <FormActions isSubmitting={isSubmitting} submitLabel="Guardar cambios" onCancel={cancel} />
    </form>
  );
}
