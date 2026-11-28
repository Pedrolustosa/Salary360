export interface EvolutionChartSeries {
  labels: string[];
  gross: (number | null)[];
  net: (number | null)[];
  benefits: (number | null)[];
  deductions: (number | null)[];
}
