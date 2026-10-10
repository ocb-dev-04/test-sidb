import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { canManageServiceRequests, transitionRequiresComment } from '../auth/permissions';
import { useToast } from '../hooks/useToast';
import { createChangeStatusSchema } from '../schemas/serviceRequestSchemas';
import type { ChangeStatusFormValues } from '../schemas/serviceRequestSchemas';
import { TRANSITION_COMMENT_MAXIMUM_LENGTH } from '../schemas/validationLimits';
import { QUERY_KEYS } from '../services/queryKeys';
import { serviceRequestService } from '../services/serviceRequestService';
import { userService } from '../services/userService';
import type { ServiceRequestDetailResponse, UserRole } from '../types/api';
import { errorMessage } from '../utils/errorMessages';
import { roleLabel, statusLabel } from '../utils/labels';
import { Card } from './Card';
import { FormField } from './FormField';
import { Alert } from './States';

interface PanelProps {
  serviceRequest: ServiceRequestDetailResponse;
}

function useRefreshServiceRequest(serviceRequestId: string) {
  const queryClient = useQueryClient();
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequestDetail(serviceRequestId) }),
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequests }),
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.dashboard }),
    ]);
}

function ChangeStatusForm({ serviceRequest }: PanelProps) {
  const { showToast } = useToast();
  const refresh = useRefreshServiceRequest(serviceRequest.id);
  const schema = useMemo(() => createChangeStatusSchema(serviceRequest.status), [serviceRequest.status]);
  const firstTransition = serviceRequest.allowedTransitions[0];
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<ChangeStatusFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { newStatus: firstTransition, comment: '' },
  });
  const selectedStatus = watch('newStatus');
  const commentRequired = selectedStatus ? transitionRequiresComment(serviceRequest.status, selectedStatus) : false;

  const changeStatus = useMutation({
    mutationFn: (values: ChangeStatusFormValues) =>
      serviceRequestService.changeStatus(serviceRequest.id, {
        newStatus: values.newStatus,
        comment: values.comment || null,
      }),
    onSuccess: async (_, values) => {
      showToast(`Estado actualizado a "${statusLabel(values.newStatus)}".`);
      await refresh();
    },
  });

  if (serviceRequest.allowedTransitions.length === 0) {
    return <p className="text-muted">No hay transiciones disponibles desde el estado actual.</p>;
  }

  return (
    <form className="stack" onSubmit={handleSubmit((values) => changeStatus.mutate(values))} noValidate>
      {changeStatus.isError && <Alert tone="error">{errorMessage(changeStatus.error)}</Alert>}
      <FormField label="Nuevo estado" required error={errors.newStatus?.message}>
        <select className="select" {...register('newStatus')}>
          {serviceRequest.allowedTransitions.map((status) => (
            <option key={status} value={status}>
              {statusLabel(status)}
            </option>
          ))}
        </select>
      </FormField>
      <FormField
        label="Comentario"
        required={commentRequired}
        error={errors.comment?.message}
        hint={commentRequired ? 'Obligatorio para resolver, cerrar o reabrir.' : 'Opcional.'}
      >
        <textarea
          className="textarea"
          rows={3}
          maxLength={TRANSITION_COMMENT_MAXIMUM_LENGTH}
          {...register('comment')}
        />
      </FormField>
      <button type="submit" className="button button--primary" disabled={changeStatus.isPending}>
        {changeStatus.isPending ? 'Actualizando…' : 'Cambiar estado'}
      </button>
    </form>
  );
}

function AssignmentForm({ serviceRequest }: PanelProps) {
  const { showToast } = useToast();
  const refresh = useRefreshServiceRequest(serviceRequest.id);
  const [assigneeId, setAssigneeId] = useState(serviceRequest.assigneeId ?? '');
  const assignees = useQuery({ queryKey: QUERY_KEYS.assignees, queryFn: userService.assignees });

  const assign = useMutation({
    mutationFn: () => serviceRequestService.assign(serviceRequest.id, assigneeId),
    onSuccess: async () => {
      showToast('Responsable asignado correctamente.');
      await refresh();
    },
  });

  const unchanged = assigneeId === '' || assigneeId === serviceRequest.assigneeId;

  return (
    <form
      className="stack"
      onSubmit={(event) => {
        event.preventDefault();
        assign.mutate();
      }}
    >
      {assign.isError && <Alert tone="error">{errorMessage(assign.error)}</Alert>}
      {assignees.isError && <Alert tone="error">{errorMessage(assignees.error)}</Alert>}
      <FormField label="Responsable técnico">
        <select
          className="select"
          value={assigneeId}
          disabled={assignees.isPending}
          onChange={(event) => setAssigneeId(event.target.value)}
        >
          <option value="">{assignees.isPending ? 'Cargando…' : 'Seleccione un responsable'}</option>
          {assignees.data?.map((assignee) => (
            <option key={assignee.id} value={assignee.id}>
              {assignee.fullName} ({roleLabel(assignee.role)})
            </option>
          ))}
        </select>
      </FormField>
      <button type="submit" className="button button--secondary" disabled={unchanged || assign.isPending}>
        {assign.isPending ? 'Asignando…' : serviceRequest.assigneeId ? 'Reasignar' : 'Asignar'}
      </button>
    </form>
  );
}

interface ServiceRequestManagementPanelProps extends PanelProps {
  role: UserRole | undefined;
}

export function ServiceRequestManagementPanel({ serviceRequest, role }: ServiceRequestManagementPanelProps) {
  if (!canManageServiceRequests(role)) {
    return null;
  }

  return (
    <>
      <Card title="Cambiar estado" subtitle={`Estado actual: ${statusLabel(serviceRequest.status)}`}>
        <ChangeStatusForm key={serviceRequest.status} serviceRequest={serviceRequest} />
      </Card>
      {serviceRequest.status !== 'Closed' && (
        <Card title="Asignación">
          <AssignmentForm key={serviceRequest.assigneeId ?? 'sin-asignar'} serviceRequest={serviceRequest} />
        </Card>
      )}
    </>
  );
}
