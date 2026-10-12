import { zodResolver } from '@hookform/resolvers/zod';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { Card } from '../components/Card';
import { FormField } from '../components/FormField';
import { Alert, ErrorState, LoadingState } from '../components/States';
import { useToast } from '../hooks/useToast';
import { ROUTES } from '../layout/routes';
import { GOVERNMENT_ENTITY_SCHEMA } from '../schemas/governmentEntitySchemas';
import type { GovernmentEntityFormValues } from '../schemas/governmentEntitySchemas';
import {
  GOVERNMENT_ENTITY_CATEGORY_MAXIMUM_LENGTH,
  GOVERNMENT_ENTITY_NAME_MAXIMUM_LENGTH,
  GOVERNMENT_ENTITY_SECTOR_MAXIMUM_LENGTH,
  GOVERNMENT_ENTITY_STATE_BRANCH_MAXIMUM_LENGTH,
} from '../schemas/validationLimits';
import { governmentEntityService } from '../services/governmentEntityService';
import { QUERY_KEYS } from '../services/queryKeys';
import { errorMessage } from '../utils/errorMessages';
import { applyServerFieldErrors } from '../utils/formErrors';

const FORM_FIELDS = ['name', 'category', 'stateBranch', 'sector'] as const;
const EMPTY_ENTITY: GovernmentEntityFormValues = { name: '', category: '', stateBranch: '', sector: '' };
const SUGGESTION_LIST_IDS = {
  category: 'government-entity-categories',
  stateBranch: 'government-entity-state-branches',
  sector: 'government-entity-sectors',
} as const;

export function GovernmentEntityFormPage() {
  const { id } = useParams();
  const entityId = id ? Number(id) : undefined;
  const isEditing = entityId !== undefined;
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const [submitError, setSubmitError] = useState<string | null>(null);

  const existing = useQuery({
    queryKey: QUERY_KEYS.governmentEntityDetail(entityId ?? 0),
    queryFn: () => governmentEntityService.detail(entityId as number),
    enabled: isEditing,
  });

  const suggestions = useQuery({
    queryKey: QUERY_KEYS.governmentEntityFilterValues,
    queryFn: governmentEntityService.filterValues,
  });

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<GovernmentEntityFormValues>({
    resolver: zodResolver(GOVERNMENT_ENTITY_SCHEMA),
    defaultValues: EMPTY_ENTITY,
  });

  useEffect(() => {
    if (existing.data) {
      const { name, category, stateBranch, sector } = existing.data;
      reset({ name, category, stateBranch, sector });
    }
  }, [existing.data, reset]);

  const backToList = () => navigate(ROUTES.governmentEntities);

  const submit = async (values: GovernmentEntityFormValues) => {
    setSubmitError(null);
    try {
      if (isEditing) {
        await governmentEntityService.update(entityId, values);
        showToast('Entidad actualizada correctamente.');
      } else {
        await governmentEntityService.create(values);
        showToast('Entidad creada correctamente.');
      }
      await queryClient.invalidateQueries({ queryKey: QUERY_KEYS.governmentEntities });
      backToList();
    } catch (error) {
      applyServerFieldErrors(error, setError, FORM_FIELDS);
      setSubmitError(errorMessage(error));
    }
  };

  if (isEditing && existing.isPending) {
    return (
      <Card>
        <LoadingState />
      </Card>
    );
  }
  if (existing.isError) {
    return (
      <Card>
        <ErrorState error={existing.error} onRetry={backToList} />
      </Card>
    );
  }

  return (
    <Card title={isEditing ? 'Editar entidad gubernamental' : 'Crear entidad gubernamental'}>
      <div className="stack">
        <form onSubmit={handleSubmit(submit)} noValidate className="stack">
          {submitError && <Alert tone="error">{submitError}</Alert>}
          <div className="form-grid form-grid--two">
            <FormField label="Nombre" required className="form-grid__wide" error={errors.name?.message}>
              <input className="input" maxLength={GOVERNMENT_ENTITY_NAME_MAXIMUM_LENGTH} {...register('name')} />
            </FormField>
            <FormField
              label="Categoría"
              required
              error={errors.category?.message}
              hint="Puede elegir una existente o escribir una nueva."
            >
              <input
                className="input"
                list={SUGGESTION_LIST_IDS.category}
                maxLength={GOVERNMENT_ENTITY_CATEGORY_MAXIMUM_LENGTH}
                {...register('category')}
              />
            </FormField>
            <FormField label="Poder del Estado" required error={errors.stateBranch?.message}>
              <input
                className="input"
                list={SUGGESTION_LIST_IDS.stateBranch}
                maxLength={GOVERNMENT_ENTITY_STATE_BRANCH_MAXIMUM_LENGTH}
                {...register('stateBranch')}
              />
            </FormField>
            <FormField label="Sector" required className="form-grid__wide" error={errors.sector?.message}>
              <input
                className="input"
                list={SUGGESTION_LIST_IDS.sector}
                maxLength={GOVERNMENT_ENTITY_SECTOR_MAXIMUM_LENGTH}
                {...register('sector')}
              />
            </FormField>
          </div>
          <datalist id={SUGGESTION_LIST_IDS.category}>
            {suggestions.data?.categories.map((value) => (
              <option key={value} value={value} />
            ))}
          </datalist>
          <datalist id={SUGGESTION_LIST_IDS.stateBranch}>
            {suggestions.data?.stateBranches.map((value) => (
              <option key={value} value={value} />
            ))}
          </datalist>
          <datalist id={SUGGESTION_LIST_IDS.sector}>
            {suggestions.data?.sectors.map((value) => (
              <option key={value} value={value} />
            ))}
          </datalist>
          <div className="form-actions">
            <button type="button" className="button button--secondary" onClick={backToList} disabled={isSubmitting}>
              Cancelar
            </button>
            <button type="submit" className="button button--primary" disabled={isSubmitting}>
              {isSubmitting ? 'Guardando…' : isEditing ? 'Guardar cambios' : 'Crear entidad'}
            </button>
          </div>
        </form>
      </div>
    </Card>
  );
}
