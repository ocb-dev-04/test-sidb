import { useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Card } from '../components/Card';
import { ServiceRequestForm } from '../components/ServiceRequestForm';
import { useToast } from '../hooks/useToast';
import { ROUTES } from '../layout/routes';
import { EMPTY_SERVICE_REQUEST_FORM } from '../schemas/serviceRequestSchemas';
import type { ServiceRequestFormValues } from '../schemas/serviceRequestSchemas';
import { QUERY_KEYS } from '../services/queryKeys';
import { serviceRequestService } from '../services/serviceRequestService';

export function ServiceRequestCreatePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  const create = async (values: ServiceRequestFormValues) => {
    const created = await serviceRequestService.create({
      ...values,
      evidenceReference: values.evidenceReference || null,
    });
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequests }),
      queryClient.invalidateQueries({ queryKey: QUERY_KEYS.dashboard }),
    ]);
    showToast(`Solicitud ${created.code} registrada correctamente.`);
    navigate(ROUTES.serviceRequestDetail(created.id));
  };

  return (
    <Card
      title="Crear solicitud"
      subtitle="Los campos marcados con * son obligatorios. La fecha compromiso se calcula según la prioridad."
    >
      <ServiceRequestForm
        defaultValues={EMPTY_SERVICE_REQUEST_FORM}
        submitLabel="Registrar solicitud"
        onSubmit={create}
        onCancel={() => navigate(ROUTES.serviceRequests)}
      />
    </Card>
  );
}
