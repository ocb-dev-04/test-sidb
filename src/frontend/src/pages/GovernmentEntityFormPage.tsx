import { zodResolver } from '@hookform/resolvers/zod';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { Card } from '../components/Card';
import { FormField } from '../components/FormField';
import { SourceSelector } from '../components/SourceSelector';
import { Alert, ErrorState, LoadingState } from '../components/States';
import { PLACEHOLDER_SOURCE, SOURCE_PARAMETER, useGovernmentEntitySource } from '../hooks/useGovernmentEntitySource';
import { useToast } from '../hooks/useToast';
import type { ListReturnState } from '../hooks/useListSearchParameters';
import { useUnsavedChangesGuard } from '../hooks/useUnsavedChangesGuard';
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
import type { GovernmentEntityDataSource } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { applyServerFieldErrors } from '../utils/formErrors';
import { dataSourceLabel } from '../utils/labels';

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
  const location = useLocation();
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const sourceSelection = useGovernmentEntitySource();
  const writableSources = sourceSelection.sources.filter((source) => !source.isReadOnly);
  const selected =
    sourceSelection.selected && !sourceSelection.selected.isReadOnly ? sourceSelection.selected : writableSources[0];
  const source = selected?.source;
  const [submitError, setSubmitError] = useState<string | null>(null);

  const existing = useQuery({
    queryKey: QUERY_KEYS.governmentEntityDetail(entityId ?? 0, source ?? PLACEHOLDER_SOURCE),
    queryFn: () => governmentEntityService.detail(entityId as number, source as GovernmentEntityDataSource),
    enabled: isEditing && source !== undefined,
  });

  const suggestions = useQuery({
    queryKey: QUERY_KEYS.governmentEntityFilterValues(source ?? PLACEHOLDER_SOURCE),
    queryFn: () => governmentEntityService.filterValues(source as GovernmentEntityDataSource),
    enabled: source !== undefined,
  });

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting, isDirty },
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

  const listSearch =
    (location.state as ListReturnState | null)?.listSearch ?? (source ? `${SOURCE_PARAMETER}=${source}` : '');
  const backToList = () =>
    navigate(listSearch ? `${ROUTES.governmentEntities}?${listSearch}` : ROUTES.governmentEntities);
  const { prompt } = useUnsavedChangesGuard(isDirty, isSubmitting, backToList);

  const submit = async (values: GovernmentEntityFormValues) => {
    if (!source) {
      return;
    }
    setSubmitError(null);
    try {
      if (isEditing) {
        await governmentEntityService.update(entityId, source, values);
        showToast('Entidad actualizada correctamente.');
      } else {
        await governmentEntityService.create(source, values);
        showToast('Entidad creada correctamente.');
      }
      await queryClient.invalidateQueries({ queryKey: QUERY_KEYS.governmentEntities });
      backToList();
    } catch (error) {
      applyServerFieldErrors(error, setError, FORM_FIELDS);
      setSubmitError(errorMessage(error));
    }
  };

  if (sourceSelection.isPending || (isEditing && existing.isPending && source)) {
    return (
      <Card>
        <LoadingState />
      </Card>
    );
  }
  if (sourceSelection.error || existing.isError) {
    return (
      <Card>
        <ErrorState error={sourceSelection.error ?? existing.error} onRetry={backToList} />
      </Card>
    );
  }
  if (!source) {
    return (
      <Card>
        <Alert tone="warning">No hay fuentes de datos editables disponibles.</Alert>
      </Card>
    );
  }

  return (
    <Card
      title={isEditing ? 'Editar entidad gubernamental' : 'Crear entidad gubernamental'}
      subtitle={`Los cambios se guardarán en la fuente ${dataSourceLabel(source)}.`}
    >
      <div className="stack">
        {!isEditing && (
          <SourceSelector
            sources={sourceSelection.sources}
            selected={source}
            onSelect={sourceSelection.selectSource}
            writableOnly
          />
        )}
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
        {prompt}
      </div>
    </Card>
  );
}
