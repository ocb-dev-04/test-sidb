import { zodResolver } from '@hookform/resolvers/zod';
import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { ActiveBadge } from '../components/Badges';
import { Card } from '../components/Card';
import { Dialog } from '../components/Dialog';
import { FormField } from '../components/FormField';
import { Alert, EmptyState, ErrorState, LoadingState } from '../components/States';
import { useAreas, useRequestTypes } from '../hooks/useCatalogs';
import { useToast } from '../hooks/useToast';
import { CATALOG_ITEM_SCHEMA } from '../schemas/catalogSchemas';
import type { CatalogItemFormValues } from '../schemas/catalogSchemas';
import { CATALOG_NAME_MAXIMUM_LENGTH, REQUEST_TYPE_DESCRIPTION_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { catalogService } from '../services/catalogService';
import { QUERY_KEYS } from '../services/queryKeys';
import { errorMessage } from '../utils/errorMessages';
import { applyServerFieldErrors } from '../utils/formErrors';

type CatalogKind = 'areas' | 'requestTypes';

const CATALOG_TABS: Record<CatalogKind, { label: string; singular: string; createLabel: string }> = {
  areas: { label: 'Áreas', singular: 'área', createLabel: 'Nueva área' },
  requestTypes: { label: 'Tipos de solicitud', singular: 'tipo de solicitud', createLabel: 'Nuevo tipo de solicitud' },
};

const CATALOG_FIELDS = ['name', 'description', 'isActive'] as const;
const INCLUDE_INACTIVE = true;

interface CatalogRow {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

interface CatalogEditorState {
  kind: CatalogKind;
  item: CatalogRow | null;
}

interface CatalogItemFormProps {
  kind: CatalogKind;
  item: CatalogRow | null;
  onSubmit: (values: CatalogItemFormValues) => Promise<void>;
  onCancel: () => void;
}

function CatalogItemForm({ kind, item, onSubmit, onCancel }: CatalogItemFormProps) {
  const [submitError, setSubmitError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<CatalogItemFormValues>({
    resolver: zodResolver(CATALOG_ITEM_SCHEMA),
    defaultValues: { name: item?.name ?? '', description: item?.description ?? '', isActive: item?.isActive ?? true },
  });

  const submit = async (values: CatalogItemFormValues) => {
    setSubmitError(null);
    try {
      await onSubmit(values);
    } catch (error) {
      applyServerFieldErrors(error, setError, CATALOG_FIELDS);
      setSubmitError(errorMessage(error));
    }
  };

  return (
    <form className="stack" onSubmit={handleSubmit(submit)} noValidate>
      {submitError && <Alert tone="error">{submitError}</Alert>}
      <FormField label="Nombre" required error={errors.name?.message}>
        <input className="input" maxLength={CATALOG_NAME_MAXIMUM_LENGTH} {...register('name')} />
      </FormField>
      {kind === 'requestTypes' && (
        <FormField label="Descripción" error={errors.description?.message}>
          <textarea
            className="textarea"
            rows={3}
            maxLength={REQUEST_TYPE_DESCRIPTION_MAXIMUM_LENGTH}
            {...register('description')}
          />
        </FormField>
      )}
      {item && (
        <label className="checkbox">
          <input type="checkbox" {...register('isActive')} />
          Activo (disponible para nuevas solicitudes)
        </label>
      )}
      <div className="form-actions">
        <button type="button" className="button button--secondary" onClick={onCancel} disabled={isSubmitting}>
          Cancelar
        </button>
        <button type="submit" className="button button--primary" disabled={isSubmitting}>
          {isSubmitting ? 'Guardando…' : item ? 'Guardar cambios' : 'Crear'}
        </button>
      </div>
    </form>
  );
}

export function CatalogsPage() {
  const [activeTab, setActiveTab] = useState<CatalogKind>('areas');
  const [editor, setEditor] = useState<CatalogEditorState | null>(null);
  const queryClient = useQueryClient();
  const { showToast } = useToast();
  const areas = useAreas(INCLUDE_INACTIVE);
  const requestTypes = useRequestTypes(INCLUDE_INACTIVE);
  const current = activeTab === 'areas' ? areas : requestTypes;
  const rows: CatalogRow[] = current.data ?? [];

  const save = async (values: CatalogItemFormValues) => {
    if (!editor) {
      return;
    }
    const { kind, item } = editor;
    if (kind === 'areas') {
      await (item
        ? catalogService.updateArea(item.id, { name: values.name, isActive: values.isActive })
        : catalogService.createArea({ name: values.name }));
    } else {
      await (item
        ? catalogService.updateRequestType(item.id, values)
        : catalogService.createRequestType({ name: values.name, description: values.description }));
    }
    await queryClient.invalidateQueries({ queryKey: QUERY_KEYS.catalogs });
    setEditor(null);
    showToast(item ? 'Registro actualizado.' : 'Registro creado.');
  };

  return (
    <Card
      title="Catálogos"
      subtitle="Áreas y tipos de solicitud disponibles al registrar solicitudes."
      actions={
        <button
          type="button"
          className="button button--accent"
          onClick={() => setEditor({ kind: activeTab, item: null })}
        >
          {CATALOG_TABS[activeTab].createLabel}
        </button>
      }
    >
      <div className="tabs" role="tablist">
        {(Object.keys(CATALOG_TABS) as CatalogKind[]).map((kind) => (
          <button
            key={kind}
            type="button"
            role="tab"
            className="tabs__tab"
            aria-selected={activeTab === kind}
            onClick={() => setActiveTab(kind)}
          >
            {CATALOG_TABS[kind].label}
          </button>
        ))}
      </div>

      {current.isPending && <LoadingState />}
      {current.isError && <ErrorState error={current.error} onRetry={() => current.refetch()} />}
      {current.data && rows.length === 0 && <EmptyState title="No hay registros" />}
      {rows.length > 0 && (
        <div className="table-wrapper" role="tabpanel">
          <table className="table">
            <thead>
              <tr>
                <th scope="col">Nombre</th>
                {activeTab === 'requestTypes' && <th scope="col">Descripción</th>}
                <th scope="col">Estado</th>
                <th scope="col">
                  <span className="visually-hidden">Acciones</span>
                </th>
              </tr>
            </thead>
            <tbody>
              {rows.map((row) => (
                <tr key={row.id}>
                  <td>
                    <strong>{row.name}</strong>
                  </td>
                  {activeTab === 'requestTypes' && <td className="table__muted">{row.description || '—'}</td>}
                  <td>
                    <ActiveBadge isActive={row.isActive} />
                  </td>
                  <td>
                    <div className="table__actions">
                      <button
                        type="button"
                        className="button button--secondary button--small"
                        aria-label={`Editar ${row.name}`}
                        onClick={() => setEditor({ kind: activeTab, item: row })}
                      >
                        Editar
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {editor && (
        <Dialog
          title={editor.item ? `Editar ${CATALOG_TABS[editor.kind].singular}` : CATALOG_TABS[editor.kind].createLabel}
          onClose={() => setEditor(null)}
        >
          <CatalogItemForm kind={editor.kind} item={editor.item} onSubmit={save} onCancel={() => setEditor(null)} />
        </Dialog>
      )}
    </Card>
  );
}
