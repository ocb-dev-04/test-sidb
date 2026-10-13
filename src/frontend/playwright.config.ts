import { defineConfig, devices } from '@playwright/test';

// Las pruebas e2e corren contra el stack de Docker (`docker compose up`): frontend en nginx con el proxy /api.
const DEFAULT_BASE_URL = 'http://localhost:8080';
const TEST_TIMEOUT_MILLISECONDS = 60_000;
const EXPECT_TIMEOUT_MILLISECONDS = 10_000;
const VIEWPORT = { width: 1280, height: 720 };

export default defineConfig({
  testDir: './e2e',
  outputDir: './e2e-results',
  timeout: TEST_TIMEOUT_MILLISECONDS,
  expect: { timeout: EXPECT_TIMEOUT_MILLISECONDS },
  // Comparten la base de datos del stack: en serie para que los datos de un caso no interfieran con otro.
  workers: 1,
  fullyParallel: false,
  forbidOnly: Boolean(process.env.CI),
  reporter: [
    ['list'],
    ['json', { outputFile: './e2e-results/results.json' }],
    ['html', { open: 'never', outputFolder: './e2e-report' }],
  ],
  use: {
    baseURL: process.env.E2E_BASE_URL ?? DEFAULT_BASE_URL,
    locale: 'es-DO',
    timezoneId: 'America/Santo_Domingo',
    viewport: VIEWPORT,
    video: { mode: 'on', size: VIEWPORT },
    screenshot: 'only-on-failure',
    trace: 'retain-on-failure',
  },
  projects: [
    {
      name: 'navegador',
      // E2E_CHANNEL=msedge usa el Edge instalado; sin valor, el Chromium de `npx playwright install`.
      use: { ...devices['Desktop Chrome'], viewport: VIEWPORT, channel: process.env.E2E_CHANNEL || undefined },
    },
  ],
});
