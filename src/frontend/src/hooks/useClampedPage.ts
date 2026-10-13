import { useEffect, useRef } from 'react';
import type { PagedResult } from '../types/api';
import { FIRST_PAGE } from '../utils/pagination';

// Al borrar o filtrar, la página actual puede quedar fuera de rango y la lista se vería vacía sin paginación.
export function useClampedPage<TItem>(
  result: PagedResult<TItem> | undefined,
  onPageChange: (page: number) => void,
): void {
  const onPageChangeReference = useRef(onPageChange);
  onPageChangeReference.current = onPageChange;
  const page = result?.page;
  const lastPage = Math.max(result?.totalPages ?? FIRST_PAGE, FIRST_PAGE);

  useEffect(() => {
    if (page !== undefined && page > lastPage) {
      onPageChangeReference.current(lastPage);
    }
  }, [page, lastPage]);
}
