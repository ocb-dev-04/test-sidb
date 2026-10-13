import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { canEditServiceRequest } from '../auth/permissions';
import { OverdueBadge, PriorityBadge, StatusBadge } from '../components/Badges';
import { Card } from '../components/Card';
import { CommentsSection } from '../components/CommentsSection';
import { Dialog } from '../components/Dialog';
import { ServiceRequestForm } from '../components/ServiceRequestForm';
import { ServiceRequestManagementPanel } from '../components/ServiceRequestManagementPanel';
import { ErrorState, LoadingState } from '../components/States';
import { StatusHistoryTimeline } from '../components/StatusHistoryTimeline';
import { useAuth } from '../hooks/useAuth';
import { useConcurrencyRecovery } from '../hooks/useConcurrencyRecovery';
import { useToast } from '../hooks/useToast';
import { ROUTES } from '../layout/routes';
import type { ServiceRequestFormValues } from '../schemas/serviceRequestSchemas';
import { QUERY_KEYS } from '../services/queryKeys';
import { serviceRequestService } from '../services/serviceRequestService';
import type { ServiceRequestDetailResponse, UpdateServiceRequestRequest } from '../types/api';
import { buildPartialUpdate } from '../utils/partialUpdate';
import { formatDateTime } from '../utils/formatting';

function toFormValues(serviceRequest: ServiceRequestDetailResponse): ServiceRequestFormValues {
  return {
    title: serviceRequest.title,
    description: serviceRequest.description,
    priority: serviceRequest.priority,
    areaId: serviceRequest.areaId,
    requestTypeId: serviceRequest.requestTypeId,
    evidenceReference: serviceRequest.evidenceReference ?? '',
  };
}

interface DetailFieldProps {
  label: string;
  children: React.ReactNode;
}

function DetailField({ label, children }: DetailFieldProps) {
  return (
    <div>
      <dt>{label}</dt>
      <dd>{children}</dd>
    </div>
  );
}

export function ServiceRequestDetailPage() {
  const { id = '' } = useParams();
  const { user } = useAuth();
  const { showToast } = useToast();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);
  const recoverFromConflict = useConcurrencyRecovery(id);
  const detail = useQuery({
    queryKey: QUERY_KEYS.serviceRequestDetail(id),
    queryFn: () => serviceRequestService.detail(id),
    enabled: id !== '',
  });

  if (detail.isPending) {
    return (
      <Card>
        <LoadingState message="Cargando solicitud…" />
      </Card>
    );
  }
  if (detail.isError) {
    return (
      <Card>
        <ErrorState error={detail.error} onRetry={() => detail.refetch()} />
        <div className="state">
          <Link to={ROUTES.serviceRequests}>Volver a la consulta</Link>
        </div>
      </Card>
    );
  }

  const serviceRequest = detail.data;
  const canEdit = canEditServiceRequest(user?.role, user?.id, serviceRequest);
  const originalValues = toFormValues(serviceRequest);

  const saveChanges = async (values: ServiceRequestFormValues) => {
    const changes = buildPartialUpdate(originalValues, values);
    if (Object.keys(changes).length > 0) {
      const request: UpdateServiceRequestRequest = { ...changes, version: serviceRequest.version };
      try {
        await serviceRequestService.update(serviceRequest.id, request);
      } catch (error) {
        await recoverFromConflict(error);
        throw error;
      }
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequestDetail(serviceRequest.id) }),
        queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequests }),
        queryClient.invalidateQueries({ queryKey: QUERY_KEYS.dashboard }),
      ]);
      showToast('Solicitud actualizada.');
    }
    setIsEditing(false);
  };

  return (
    <>
      <Card
        title={`${serviceRequest.code} · ${serviceRequest.title}`}
        subtitle={
          <span className="inline">
            <StatusBadge status={serviceRequest.status} />
            <PriorityBadge priority={serviceRequest.priority} />
            {serviceRequest.isOverdue && <OverdueBadge />}
          </span>
        }
        actions={
          <>
            <Link className="button button--ghost" to={ROUTES.serviceRequests}>
              Volver
            </Link>
            {canEdit && (
              <button type="button" className="button button--secondary" onClick={() => setIsEditing(true)}>
                Editar
              </button>
            )}
          </>
        }
      >
        <dl className="description-list">
          <DetailField label="Área">{serviceRequest.areaName}</DetailField>
          <DetailField label="Tipo de solicitud">{serviceRequest.requestTypeName}</DetailField>
          <DetailField label="Solicitante">{serviceRequest.requesterName}</DetailField>
          <DetailField label="Responsable">{serviceRequest.assigneeName ?? 'Sin asignar'}</DetailField>
          <DetailField label="Creada">{formatDateTime(serviceRequest.createdOnUtc)}</DetailField>
          <DetailField label="Fecha compromiso">
            <span className={serviceRequest.isOverdue ? 'text-danger' : ''}>
              {formatDateTime(serviceRequest.dueDateUtc)}
            </span>
          </DetailField>
          <DetailField label="Última actualización">{formatDateTime(serviceRequest.updatedOnUtc)}</DetailField>
          {serviceRequest.resolvedOnUtc && (
            <DetailField label="Resuelta">{formatDateTime(serviceRequest.resolvedOnUtc)}</DetailField>
          )}
          {serviceRequest.closedOnUtc && (
            <DetailField label="Cerrada">{formatDateTime(serviceRequest.closedOnUtc)}</DetailField>
          )}
        </dl>
      </Card>

      <div className="detail-grid">
        <div className="stack">
          <Card title="Descripción">
            <p className="prose">{serviceRequest.description}</p>
            {serviceRequest.evidenceReference && (
              <p>
                <strong>Evidencia: </strong>
                <span className="prose">{serviceRequest.evidenceReference}</span>
              </p>
            )}
            {serviceRequest.resolutionComment && (
              <p>
                <strong>Comentario de resolución: </strong>
                <span className="prose">{serviceRequest.resolutionComment}</span>
              </p>
            )}
          </Card>
          <Card title="Comentarios">
            <CommentsSection
              serviceRequestId={serviceRequest.id}
              comments={serviceRequest.comments}
              role={user?.role}
            />
          </Card>
        </div>
        <div className="stack">
          <ServiceRequestManagementPanel serviceRequest={serviceRequest} role={user?.role} />
          <Card title="Historial de estados">
            <StatusHistoryTimeline history={serviceRequest.statusHistory} />
          </Card>
        </div>
      </div>

      {isEditing && (
        <Dialog title={`Editar ${serviceRequest.code}`} onClose={() => setIsEditing(false)} wide>
          <ServiceRequestForm
            defaultValues={originalValues}
            submitLabel="Guardar cambios"
            onSubmit={saveChanges}
            onCancel={() => setIsEditing(false)}
          />
        </Dialog>
      )}
    </>
  );
}
