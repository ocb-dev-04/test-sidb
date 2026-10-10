import type { ProblemDetails } from '../types/api';

export class ApiError extends Error {
  readonly status: number;
  readonly code?: string;
  readonly detail?: string;
  readonly fieldErrors: Record<string, string[]>;

  constructor(status: number, problem: ProblemDetails | null) {
    super(problem?.detail ?? problem?.title ?? `HTTP ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.code = problem?.code;
    this.detail = problem?.detail ?? problem?.title;
    this.fieldErrors = problem?.errors ?? {};
  }
}

export function isApiError(value: unknown): value is ApiError {
  return value instanceof ApiError;
}
