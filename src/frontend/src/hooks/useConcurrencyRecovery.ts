import { useQueryClient } from '@tanstack/react-query';
import { useCallback } from 'react';
import { QUERY_KEYS } from '../services/queryKeys';
import { errorMessage, isConcurrencyConflict } from '../utils/errorMessages';
import { useToast } from './useToast';

// Ante un conflicto de versión recarga la solicitud para que el usuario decida sobre los datos actuales.
export function useConcurrencyRecovery(serviceRequestId: string): (error: unknown) => Promise<void> {
  const queryClient = useQueryClient();
  const { showToast } = useToast();

  return useCallback(
    async (error: unknown) => {
      if (!isConcurrencyConflict(error)) {
        return;
      }
      showToast(errorMessage(error), 'warning');
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequestDetail(serviceRequestId) }),
        queryClient.invalidateQueries({ queryKey: QUERY_KEYS.serviceRequests }),
      ]);
    },
    [queryClient, serviceRequestId, showToast],
  );
}
