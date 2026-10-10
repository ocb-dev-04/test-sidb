import type { StatusHistoryResponse } from '../types/api';
import { formatDateTime } from '../utils/formatting';
import { StatusBadge } from './Badges';
import { EmptyState } from './States';

export function StatusHistoryTimeline({ history }: { history: StatusHistoryResponse[] }) {
  if (history.length === 0) {
    return <EmptyState title="Sin cambios de estado registrados" />;
  }

  const orderedHistory = [...history].sort(
    (first, second) => new Date(second.changedOnUtc).getTime() - new Date(first.changedOnUtc).getTime(),
  );

  return (
    <ol className="timeline">
      {orderedHistory.map((entry) => (
        <li key={entry.id} className="timeline__item">
          <div className="inline">
            {entry.previousStatus !== entry.newStatus && (
              <>
                <StatusBadge status={entry.previousStatus} />
                <span aria-label="cambió a">→</span>
              </>
            )}
            <StatusBadge status={entry.newStatus} />
          </div>
          <div className="timeline__meta">
            {entry.changedByName} · {formatDateTime(entry.changedOnUtc)}
          </div>
          {entry.comment && <div className="timeline__comment prose">{entry.comment}</div>}
        </li>
      ))}
    </ol>
  );
}
