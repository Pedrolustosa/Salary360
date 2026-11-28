export interface ProjectionChartData {
  labels: string[];
  actual: (number | null)[];
  conservative: (number | null)[];
  likely: (number | null)[];
  optimistic: (number | null)[];
}
