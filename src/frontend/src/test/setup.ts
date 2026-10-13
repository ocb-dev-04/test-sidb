import '@testing-library/jest-dom/vitest';
import { cleanup, configure } from '@testing-library/react';
import { afterAll, afterEach, beforeAll } from 'vitest';
import { server } from './server';

// Con la suite en paralelo en Windows, el segundo por defecto de findBy/waitFor se queda corto.
const ASYNC_UTILITY_TIMEOUT_MILLISECONDS = 5000;

configure({ asyncUtilTimeout: ASYNC_UTILITY_TIMEOUT_MILLISECONDS });

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));

afterEach(() => {
  cleanup();
  server.resetHandlers();
  sessionStorage.clear();
});

afterAll(() => server.close());
