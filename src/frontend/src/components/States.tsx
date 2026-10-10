import type { ReactNode } from 'react';
import { errorMessage } from '../utils/errorMessages';

export function LoadingState({ message = 'Cargando…' }: { message?: string }) {
  return (
    <div className="state" role="status" aria-live="polite">
      <span className="spinner" aria-hidden="true" />
      <span>{message}</span>
    </div>
  );
}

interface EmptyStateProps {
  title: string;
  description?: string;
  action?: ReactNode;
}

export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="state">
      <span className="state__title">{title}</span>
      {description && <span>{description}</span>}
      {action}
    </div>
  );
}

interface ErrorStateProps {
  error: unknown;
  onRetry?: () => void;
}

export function ErrorState({ error, onRetry }: ErrorStateProps) {
  return (
    <div className="state" role="alert">
      <span className="state__title">No se pudo cargar la información</span>
      <span>{errorMessage(error)}</span>
      {onRetry && (
        <button type="button" className="button button--secondary" onClick={onRetry}>
          Reintentar
        </button>
      )}
    </div>
  );
}

interface AlertProps {
  tone: 'error' | 'success' | 'info' | 'warning';
  children: ReactNode;
}

export function Alert({ tone, children }: AlertProps) {
  return (
    <div className={`alert alert--${tone}`} role={tone === 'error' ? 'alert' : 'status'}>
      {children}
    </div>
  );
}
