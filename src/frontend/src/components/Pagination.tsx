import { formatNumber } from '../utils/formatting';
import { FIRST_PAGE, PAGE_SIZE_OPTIONS } from '../utils/pagination';

interface PaginationProps {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
}

export function Pagination({
  page,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
  onPageSizeChange,
}: PaginationProps) {
  const firstItem = totalCount === 0 ? 0 : (page - 1) * pageSize + 1;
  const lastItem = Math.min(page * pageSize, totalCount);

  return (
    <nav className="pagination" aria-label="Paginación">
      <span>
        Mostrando {formatNumber(firstItem)}–{formatNumber(lastItem)} de {formatNumber(totalCount)} registros
      </span>
      <div className="pagination__controls">
        {onPageSizeChange && (
          <label className="pagination__size">
            Filas
            <select
              className="select"
              value={pageSize}
              onChange={(event) => onPageSizeChange(Number(event.target.value))}
            >
              {PAGE_SIZE_OPTIONS.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </label>
        )}
        <button
          type="button"
          className="button button--secondary button--small"
          disabled={page <= FIRST_PAGE}
          onClick={() => onPageChange(page - 1)}
        >
          Anterior
        </button>
        <span>
          Página {page} de {Math.max(totalPages, FIRST_PAGE)}
        </span>
        <button
          type="button"
          className="button button--secondary button--small"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Siguiente
        </button>
      </div>
    </nav>
  );
}
