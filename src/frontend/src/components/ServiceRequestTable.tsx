import { Link, useNavigate } from 'react-router-dom';
import { ROUTES } from '../layout/routes';
import type { ServiceRequestSummaryResponse } from '../types/api';
import { formatDate } from '../utils/formatting';
import { OverdueBadge, PriorityBadge, StatusBadge } from './Badges';

interface ServiceRequestTableProps {
  items: ServiceRequestSummaryResponse[];
  showRequester?: boolean;
}

export function ServiceRequestTable({ items, showRequester = true }: ServiceRequestTableProps) {
  const navigate = useNavigate();

  return (
    <div className="table-wrapper">
      <table className="table">
        <thead>
          <tr>
            <th scope="col">Código</th>
            <th scope="col">Solicitud</th>
            <th scope="col">Estado</th>
            <th scope="col">Prioridad</th>
            <th scope="col">{showRequester ? 'Solicitante / Responsable' : 'Responsable'}</th>
            <th scope="col">Compromiso</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr
              key={item.id}
              className="table__row--clickable"
              onClick={() => navigate(ROUTES.serviceRequestDetail(item.id))}
            >
              <td className="table__code">
                <Link to={ROUTES.serviceRequestDetail(item.id)} onClick={(event) => event.stopPropagation()}>
                  {item.code}
                </Link>
              </td>
              <td className="table__title">
                {item.title}
                <div className="table__muted">
                  {item.areaName} · {item.requestTypeName}
                </div>
              </td>
              <td>
                <StatusBadge status={item.status} />
              </td>
              <td>
                <PriorityBadge priority={item.priority} />
              </td>
              <td>
                {showRequester && <div>{item.requesterName}</div>}
                <div className={showRequester ? 'table__muted' : ''}>{item.assigneeName ?? 'Sin asignar'}</div>
              </td>
              <td>
                <div className="inline">
                  <span className={item.isOverdue ? 'text-danger' : ''}>{formatDate(item.dueDateUtc)}</span>
                  {item.isOverdue && <OverdueBadge />}
                </div>
                <div className="table__muted">Creada {formatDate(item.createdOnUtc)}</div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
