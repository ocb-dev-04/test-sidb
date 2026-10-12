import { setupServer } from 'msw/node';
import { API_BASE_URL } from '../services/configuration';

export const server = setupServer();

export function apiUrl(path: string): string {
  return `${API_BASE_URL}${path}`;
}
