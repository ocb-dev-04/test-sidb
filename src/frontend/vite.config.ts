/// <reference types="vitest" />
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

const DEVELOPMENT_PORT = 5173;
const DEFAULT_API_PROXY_TARGET = 'http://localhost:5080';

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), '');
  const apiProxy = {
    '/api': { target: environment.API_PROXY_TARGET || DEFAULT_API_PROXY_TARGET, changeOrigin: true },
  };

  return {
    plugins: [react()],
    server: { port: DEVELOPMENT_PORT, strictPort: true, proxy: apiProxy },
    preview: { proxy: apiProxy },
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      css: false,
    },
  };
});
