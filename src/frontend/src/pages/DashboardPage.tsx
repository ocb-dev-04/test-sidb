import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { BarChart } from '../components/BarChart';
import type { BarChartItem } from '../components/BarChart';
import { Card } from '../components/Card';
import { ServiceRequestTable } from '../components/ServiceRequestTable';
import { EmptyState, ErrorState, LoadingState } from '../components/States';
import { useAuth } from '../hooks/useAuth';
import { ROUTES } from '../layout/routes';
import { dashboardService } from '../services/dashboardService';
import { QUERY_KEYS } from '../services/queryKeys';
import type { ServiceRequestPriority, ServiceRequestStatus } from '../types/api';
import { SERVICE_REQUEST_PRIORITIES, SERVICE_REQUEST_STATUSES } from '../types/api';
import { formatNumber } from '../utils/formatting';
import { priorityLabel, statusLabel } from '../utils/labels';

const STATUS_COLORS: Record<ServiceRequestStatus, string> = {
  Registered: 'var(--color-neutral)',
  InAnalysis: 'var(--color-info)',
  InProgress: 'var(--color-violet)',
  WaitingForRequester: 'var(--color-warning)',
  Resolved: 'var(--color-success)',
  Closed: 'var(--color-brand-base)',
};

const PRIORITY_COLORS: Record<ServiceRequestPriority, string> = {
  Low: 'var(--color-neutral)',
  Medium: 'var(--color-info)',
  High: 'var(--color-accent)',
  Critical: 'var(--color-danger)',
};

interface KpiProps {
  label: string;
  value: number;
  color: string;
}

function Kpi({ label, value, color }: KpiProps) {
  return (
    <div className="kpi" style={{ ['--kpi-color' as string]: color }}>
      <span className="kpi__label">{label}</span>
      <span className="kpi__value">{formatNumber(value)}</span>
    </div>
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const summary = useQuery({ queryKey: QUERY_KEYS.dashboard, queryFn: dashboardService.summary });

  if (summary.isPending) {
    return (
      <Card>
        <LoadingState message="Cargando resumen…" />
      </Card>
    );
  }
  if (summary.isError) {
    return (
      <Card>
        <ErrorState error={summary.error} onRetry={() => summary.refetch()} />
      </Card>
    );
  }

  const data = summary.data;
  const statusItems: BarChartItem[] = SERVICE_REQUEST_STATUSES.map((status) => ({
    key: status,
    label: statusLabel(status),
    value: data.byStatus.find((item) => item.status === status)?.count ?? 0,
    color: STATUS_COLORS[status],
  }));
  const priorityItems: BarChartItem[] = SERVICE_REQUEST_PRIORITIES.map((priority) => ({
    key: priority,
    label: priorityLabel(priority),
    value: data.byPriority.find((item) => item.priority === priority)?.count ?? 0,
    color: PRIORITY_COLORS[priority],
  }));

  return (
    <>
      <Card title={`Bienvenido, ${user?.fullName ?? ''}`} subtitle="Resumen de las solicitudes visibles para su rol.">
        <div className="kpi-grid">
          <Kpi label="Total" value={data.totalCount} color="var(--color-brand-base)" />
          <Kpi label="Abiertas" value={data.openCount} color="var(--color-info)" />
          <Kpi label="Cerradas" value={data.closedCount} color="var(--color-success)" />
          <Kpi label="Vencidas" value={data.overdueCount} color="var(--color-accent)" />
        </div>
      </Card>
      <div className="charts-grid">
        <Card title="Solicitudes por estado">
          <BarChart items={statusItems} label="Solicitudes por estado" />
        </Card>
        <Card title="Solicitudes por prioridad">
          <BarChart items={priorityItems} label="Solicitudes por prioridad" />
        </Card>
      </div>
      <Card
        title="Últimas solicitudes"
        actions={
          <Link className="button button--secondary button--small" to={ROUTES.serviceRequests}>
            Ver todas
          </Link>
        }
      >
        {data.latestRequests.length === 0 ? (
          <EmptyState
            title="Aún no hay solicitudes"
            action={
              <Link className="button button--primary" to={ROUTES.newServiceRequest}>
                Crear solicitud
              </Link>
            }
          />
        ) : (
          <ServiceRequestTable items={data.latestRequests} />
        )}
      </Card>
    </>
  );
}
