import type { ProblemDetails } from '../types/api';
import { ApiError } from './apiError';
import { API_BASE_URL } from './configuration';
import { readSession } from './sessionStore';

const HTTP_STATUS_NO_CONTENT = 204;
const HTTP_STATUS_UNAUTHORIZED = 401;
const NETWORK_ERROR_STATUS = 0;

type QueryValue = string | number | boolean | undefined | null;
export type QueryParameters = Record<string, QueryValue>;

let unauthorizedHandler: (() => void) | null = null;

export function onUnauthorized(handler: (() => void) | null): void {
  unauthorizedHandler = handler;
}

export function buildQueryString(parameters: QueryParameters = {}): string {
  const searchParameters = new URLSearchParams();
  Object.entries(parameters).forEach(([name, value]) => {
    if (value === undefined || value === null || value === '') {
      return;
    }
    searchParameters.append(name, String(value));
  });
  const queryString = searchParameters.toString();
  return queryString ? `?${queryString}` : '';
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  query?: QueryParameters;
  body?: unknown;
  authenticated?: boolean;
}

async function readProblem(response: Response): Promise<ProblemDetails | null> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    return null;
  }
}

export async function request<TResponse>(path: string, options: RequestOptions = {}): Promise<TResponse> {
  const { method = 'GET', query, body, authenticated = true } = options;
  const headers: Record<string, string> = { Accept: 'application/json' };
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  const session = readSession();
  if (authenticated && session) {
    headers.Authorization = `Bearer ${session.accessToken}`;
  }

  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}${path}${buildQueryString(query)}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    throw new ApiError(NETWORK_ERROR_STATUS, { code: 'network.unavailable' });
  }

  if (!response.ok) {
    const problem = await readProblem(response);
    if (response.status === HTTP_STATUS_UNAUTHORIZED && authenticated) {
      unauthorizedHandler?.();
    }
    throw new ApiError(response.status, problem);
  }

  if (response.status === HTTP_STATUS_NO_CONTENT) {
    return undefined as TResponse;
  }
  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as TResponse;
}
