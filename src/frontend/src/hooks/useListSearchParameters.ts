import { useCallback, useEffect, useRef, useState } from 'react';
import type { Dispatch, SetStateAction } from 'react';
import { useSearchParams } from 'react-router-dom';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE, PAGE_SIZE_OPTIONS } from '../utils/pagination';
import { useDebouncedValue } from './useDebouncedValue';

export const PAGE_PARAMETER = 'pagina';
export const PAGE_SIZE_PARAMETER = 'filas';
export const SEARCH_PARAMETER = 'buscar';
const PAGING_PARAMETERS: readonly string[] = [PAGE_PARAMETER, PAGE_SIZE_PARAMETER];

export type ParameterChanges = Record<string, string | undefined>;

// Estado de navegación para que un formulario vuelva al listado con los mismos filtros y página.
export interface ListReturnState {
  listSearch: string;
}

export function readPositiveInteger(value: string | null): number | undefined {
  const parsed = Number(value);
  return value && Number.isInteger(parsed) && parsed > 0 ? parsed : undefined;
}

export function readOption<TValue extends string>(
  values: readonly TValue[],
  candidate: string | null,
): TValue | undefined {
  return values.find((value) => value === candidate);
}

function readPageSize(value: string | null): number {
  const pageSize = readPositiveInteger(value);
  return PAGE_SIZE_OPTIONS.find((option) => option === pageSize) ?? DEFAULT_PAGE_SIZE;
}

export interface ListSearchParameters {
  searchParameters: URLSearchParams;
  page: number;
  pageSize: number;
  activeFilterCount: number;
  updateParameters: (changes: ParameterChanges, resetPage?: boolean) => void;
  clearFilters: (keep?: readonly string[]) => void;
  setPage: (page: number) => void;
  setPageSize: (pageSize: number) => void;
}

// Filtros, orden y página viven en la URL: se conservan al volver desde un detalle o formulario y se pueden compartir.
export function useListSearchParameters(nonFilterParameters: readonly string[] = []): ListSearchParameters {
  const [searchParameters, setSearchParameters] = useSearchParams();

  const updateParameters = useCallback(
    (changes: ParameterChanges, resetPage = true) => {
      setSearchParameters(
        (current) => {
          const next = new URLSearchParams(current);
          Object.entries(changes).forEach(([name, value]) => {
            if (value) {
              next.set(name, value);
            } else {
              next.delete(name);
            }
          });
          if (resetPage) {
            next.delete(PAGE_PARAMETER);
          }
          return next;
        },
        { replace: true },
      );
    },
    [setSearchParameters],
  );

  const clearFilters = useCallback(
    (keep: readonly string[] = []) => {
      setSearchParameters(
        (current) => {
          const next = new URLSearchParams();
          [PAGE_SIZE_PARAMETER, ...keep].forEach((name) => {
            const value = current.get(name);
            if (value) {
              next.set(name, value);
            }
          });
          return next;
        },
        { replace: true },
      );
    },
    [setSearchParameters],
  );

  const ignoredParameters = [...PAGING_PARAMETERS, ...nonFilterParameters];
  const activeFilterCount = [...searchParameters.keys()].filter((name) => !ignoredParameters.includes(name)).length;

  return {
    searchParameters,
    page: readPositiveInteger(searchParameters.get(PAGE_PARAMETER)) ?? FIRST_PAGE,
    pageSize: readPageSize(searchParameters.get(PAGE_SIZE_PARAMETER)),
    activeFilterCount,
    updateParameters,
    clearFilters,
    setPage: (page) => updateParameters({ [PAGE_PARAMETER]: page > FIRST_PAGE ? String(page) : undefined }, false),
    setPageSize: (pageSize) => updateParameters({ [PAGE_SIZE_PARAMETER]: String(pageSize) }),
  };
}

// El texto se escribe libremente y llega a la URL tras el debounce; si la URL cambia desde fuera, el campo la sigue.
export function useSearchText(
  urlValue: string,
  updateParameters: (changes: ParameterChanges) => void,
  parameterName = SEARCH_PARAMETER,
): [string, Dispatch<SetStateAction<string>>] {
  const [searchText, setSearchText] = useState(urlValue);
  const debouncedSearch = useDebouncedValue(searchText.trim());
  const [previousUrlValue, setPreviousUrlValue] = useState(urlValue);
  const latest = useRef({ urlValue, updateParameters, parameterName });
  latest.current = { urlValue, updateParameters, parameterName };

  if (urlValue !== previousUrlValue) {
    setPreviousUrlValue(urlValue);
    if (urlValue !== debouncedSearch) {
      setSearchText(urlValue);
    }
  }

  // Solo reacciona al texto ya estabilizado; un cambio externo de la URL no debe reescribirla.
  useEffect(() => {
    const current = latest.current;
    if (debouncedSearch !== current.urlValue) {
      current.updateParameters({ [current.parameterName]: debouncedSearch });
    }
  }, [debouncedSearch]);

  return [searchText, setSearchText];
}
