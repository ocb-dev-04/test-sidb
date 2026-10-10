/// <reference types="vitest" />
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const DEVELOPMENT_PORT = 5173;

export default defineConfig({
  plugins: [react()],
  server: { port: DEVELOPMENT_PORT, strictPort: true },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    css: false,
  },
});
