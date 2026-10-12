import { useQuery } from '@tanstack/react-query';
import { useSearchParams } from 'react-router-dom';
import { governmentEntityService } from '../services/governmentEntityService';
import { QUERY_KEYS } from '../services/queryKeys';
import { GOVERNMENT_ENTITY_DATA_SOURCES } from '../types/api';
import type { GovernmentEntityDataSource, GovernmentEntityDataSourceResponse } from '../types/api';

export const SOURCE_PARAMETER = 'fuente';
// Solo arma claves de consulta mientras las fuentes no han cargado (las consultas están deshabilitadas).
export const PLACEHOLDER_SOURCE: GovernmentEntityDataSource = 'TextFile';
const SOURCES_STALE_MILLISECONDS = 10 * 60 * 1000;

function parseSource(value: string | null): GovernmentEntityDataSource | undefined {
  return GOVERNMENT_ENTITY_DATA_SOURCES.find((source) => source === value);
}

export interface SelectedSource {
  sources: GovernmentEntityDataSourceResponse[];
  selected: GovernmentEntityDataSourceResponse | undefined;
  isPending: boolean;
  error: unknown;
  refetch: () => void;
  selectSource: (source: GovernmentEntityDataSource) => void;
}

// La fuente seleccionada vive en la URL para conservarla entre la consulta y el formulario.
export function useGovernmentEntitySource(): SelectedSource {
  const [searchParameters, setSearchParameters] = useSearchParams();
  const sources = useQuery({
    queryKey: QUERY_KEYS.governmentEntitySources,
    queryFn: governmentEntityService.sources,
    staleTime: SOURCES_STALE_MILLISECONDS,
  });
  const requested = parseSource(searchParameters.get(SOURCE_PARAMETER));
  const available = sources.data ?? [];
  const selected =
    available.find((item) => item.source === requested) ?? available.find((item) => item.isDefault) ?? available[0];

  return {
    sources: available,
    selected,
    isPending: sources.isPending,
    error: sources.error,
    refetch: () => void sources.refetch(),
    selectSource: (source) => setSearchParameters({ [SOURCE_PARAMETER]: source }, { replace: true }),
  };
}
