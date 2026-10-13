import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useState } from 'react';
import { RouterProvider } from 'react-router-dom';
import { AuthProvider } from './auth/AuthProvider';
import { ToastProvider } from './components/ToastProvider';
import { createApplicationRouter } from './router';
import { isApiError } from './services/apiError';

const QUERY_STALE_MILLISECONDS = 30 * 1000;
const MAXIMUM_QUERY_RETRIES = 1;
const CLIENT_ERROR_MINIMUM_STATUS = 400;
const SERVER_ERROR_MINIMUM_STATUS = 500;

// Los errores 4xx son definitivos: reintentarlos solo retrasa el mensaje al usuario.
function shouldRetry(failureCount: number, error: unknown): boolean {
  if (isApiError(error) && error.status >= CLIENT_ERROR_MINIMUM_STATUS && error.status < SERVER_ERROR_MINIMUM_STATUS) {
    return false;
  }
  return failureCount < MAXIMUM_QUERY_RETRIES;
}

function createQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { staleTime: QUERY_STALE_MILLISECONDS, retry: shouldRetry, refetchOnWindowFocus: false },
    },
  });
}

export function App() {
  const [queryClient] = useState(createQueryClient);
  const [router] = useState(createApplicationRouter);

  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <AuthProvider>
          <RouterProvider router={router} future={{ v7_startTransition: true }} />
        </AuthProvider>
      </ToastProvider>
    </QueryClientProvider>
  );
}
