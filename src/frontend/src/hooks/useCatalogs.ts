import { useQuery } from '@tanstack/react-query';
import { catalogService } from '../services/catalogService';
import { QUERY_KEYS } from '../services/queryKeys';

const CATALOG_STALE_MILLISECONDS = 5 * 60 * 1000;

export function useAreas(includeInactive = false) {
  return useQuery({
    queryKey: QUERY_KEYS.areas(includeInactive),
    queryFn: () => catalogService.areas(includeInactive),
    staleTime: CATALOG_STALE_MILLISECONDS,
  });
}

export function useRequestTypes(includeInactive = false) {
  return useQuery({
    queryKey: QUERY_KEYS.requestTypes(includeInactive),
    queryFn: () => catalogService.requestTypes(includeInactive),
    staleTime: CATALOG_STALE_MILLISECONDS,
  });
}
