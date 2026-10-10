import type { CommentVisibility, ServiceRequestPriority, ServiceRequestStatus } from '../types/api';
import { priorityLabel, statusLabel, visibilityLabel } from '../utils/labels';

type BadgeTone = 'neutral' | 'info' | 'violet' | 'warning' | 'success' | 'danger' | 'accent' | 'dark';

const STATUS_TONES: Record<ServiceRequestStatus, BadgeTone> = {
  Registered: 'neutral',
  InAnalysis: 'info',
  InProgress: 'violet',
  WaitingForRequester: 'warning',
  Resolved: 'success',
  Closed: 'dark',
};

const PRIORITY_TONES: Record<ServiceRequestPriority, BadgeTone> = {
  Low: 'neutral',
  Medium: 'info',
  High: 'accent',
  Critical: 'danger',
};

interface BadgeProps {
  tone: BadgeTone;
  children: React.ReactNode;
  title?: string;
}

export function Badge({ tone, children, title }: BadgeProps) {
  return (
    <span className={`badge badge--${tone}`} title={title}>
      {children}
    </span>
  );
}

export function StatusBadge({ status }: { status: ServiceRequestStatus }) {
  return <Badge tone={STATUS_TONES[status]}>{statusLabel(status)}</Badge>;
}

export function PriorityBadge({ priority }: { priority: ServiceRequestPriority }) {
  return <Badge tone={PRIORITY_TONES[priority]}>{priorityLabel(priority)}</Badge>;
}

export function VisibilityBadge({ visibility }: { visibility: CommentVisibility }) {
  return <Badge tone={visibility === 'Internal' ? 'warning' : 'info'}>{visibilityLabel(visibility)}</Badge>;
}

export function OverdueBadge() {
  return (
    <Badge tone="danger" title="La fecha compromiso ya pasó y la solicitud sigue abierta">
      Vencida
    </Badge>
  );
}

export function ActiveBadge({ isActive }: { isActive: boolean }) {
  return <Badge tone={isActive ? 'success' : 'neutral'}>{isActive ? 'Activo' : 'Inactivo'}</Badge>;
}
