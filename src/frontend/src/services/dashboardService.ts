import type { DashboardSummaryResponse } from '../types/api';
import { request } from './httpClient';

export const dashboardService = {
  summary: () => request<DashboardSummaryResponse>('/api/dashboard/resumen'),
};
