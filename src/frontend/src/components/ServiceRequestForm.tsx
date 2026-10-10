import { zodResolver } from '@hookform/resolvers/zod';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useAreas, useRequestTypes } from '../hooks/useCatalogs';
import { SERVICE_REQUEST_SCHEMA } from '../schemas/serviceRequestSchemas';
import type { ServiceRequestFormValues } from '../schemas/serviceRequestSchemas';
import {
  SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH,
  SERVICE_REQUEST_EVIDENCE_MAXIMUM_LENGTH,
  SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH,
} from '../schemas/validationLimits';
import { SERVICE_REQUEST_PRIORITIES } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { applyServerFieldErrors } from '../utils/formErrors';
import { PRIORITY_LABELS } from '../utils/labels';
import { FormField } from './FormField';
import { Alert } from './States';

const FORM_FIELDS = ['title', 'description', 'priority', 'areaId', 'requestTypeId', 'evidenceReference'] as const;

interface ServiceRequestFormProps {
  defaultValues: ServiceRequestFormValues;
  submitLabel: string;
  onSubmit: (values: ServiceRequestFormValues) => Promise<void>;
  onCancel: () => void;
}

export function ServiceRequestForm({ defaultValues, submitLabel, onSubmit, onCancel }: ServiceRequestFormProps) {
  const areas = useAreas();
  const requestTypes = useRequestTypes();
  const [submitError, setSubmitError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<ServiceRequestFormValues>({ resolver: zodResolver(SERVICE_REQUEST_SCHEMA), defaultValues });

  const titleLength = watch('title')?.length ?? 0;
  const descriptionLength = watch('description')?.length ?? 0;

  const submit = async (values: ServiceRequestFormValues) => {
    setSubmitError(null);
    try {
      await onSubmit(values);
    } catch (error) {
      applyServerFieldErrors(error, setError, FORM_FIELDS);
      setSubmitError(errorMessage(error));
    }
  };

  return (
    <form onSubmit={handleSubmit(submit)} noValidate>
      {submitError && (
        <div className="stack" style={{ marginBottom: 'var(--space-4)' }}>
          <Alert tone="error">{submitError}</Alert>
        </div>
      )}
      <div className="form-grid">
        <FormField
          label="Título"
          required
          className="form-grid__wide"
          error={errors.title?.message}
          hint={`${titleLength}/${SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH} caracteres`}
        >
          <input className="input" maxLength={SERVICE_REQUEST_TITLE_MAXIMUM_LENGTH} {...register('title')} />
        </FormField>
        <FormField
          label="Descripción"
          required
          className="form-grid__wide"
          error={errors.description?.message}
          hint={`${descriptionLength}/${SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH} caracteres`}
        >
          <textarea
            className="textarea"
            rows={6}
            maxLength={SERVICE_REQUEST_DESCRIPTION_MAXIMUM_LENGTH}
            {...register('description')}
          />
        </FormField>
        <FormField label="Prioridad" required error={errors.priority?.message}>
          <select className="select" {...register('priority')}>
            {SERVICE_REQUEST_PRIORITIES.map((priority) => (
              <option key={priority} value={priority}>
                {PRIORITY_LABELS[priority]}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Área" required error={errors.areaId?.message}>
          <select className="select" disabled={areas.isPending} {...register('areaId')}>
            <option value={0}>{areas.isPending ? 'Cargando…' : 'Seleccione un área'}</option>
            {areas.data?.map((area) => (
              <option key={area.id} value={area.id}>
                {area.name}
              </option>
            ))}
          </select>
        </FormField>
        <FormField label="Tipo de solicitud" required error={errors.requestTypeId?.message}>
          <select className="select" disabled={requestTypes.isPending} {...register('requestTypeId')}>
            <option value={0}>{requestTypes.isPending ? 'Cargando…' : 'Seleccione un tipo'}</option>
            {requestTypes.data?.map((requestType) => (
              <option key={requestType.id} value={requestType.id} title={requestType.description}>
                {requestType.name}
              </option>
            ))}
          </select>
        </FormField>
        <FormField
          label="Referencia de evidencia"
          className="form-grid__wide"
          error={errors.evidenceReference?.message}
          hint="Opcional: enlace o ubicación de un documento o captura de soporte."
        >
          <input
            className="input"
            maxLength={SERVICE_REQUEST_EVIDENCE_MAXIMUM_LENGTH}
            placeholder="https://… o \\servidor\carpeta\archivo"
            {...register('evidenceReference')}
          />
        </FormField>
      </div>
      {(areas.isError || requestTypes.isError) && (
        <Alert tone="warning">No se pudieron cargar los catálogos de áreas o tipos de solicitud.</Alert>
      )}
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </button>
        <button type="submit" className="button button--primary" disabled={isSubmitting}>
          {isSubmitting ? 'Guardando…' : submitLabel}
        </button>
      </div>
    </form>
  );
}
