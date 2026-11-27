export interface ProjectionPoint {
  year: number;
  month: number;
  actual: number | null;
  conservative: number | null;
  likely: number | null;
  optimistic: number | null;
}
