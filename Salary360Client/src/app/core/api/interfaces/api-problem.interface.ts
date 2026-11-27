export interface ApiProblem {
  status?: number;
  title?: string;
  code?: string;
  codes?: string[];
  errors?: Record<string, string[]>;
  retryAfterSeconds?: number;
  traceId?: string;
}
