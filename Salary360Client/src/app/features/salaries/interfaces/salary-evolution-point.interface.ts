export interface SalaryEvolutionPoint {
  year: number;
  gross: number | null;
  benefits: number | null;
  deductions: number | null;
  net: number | null;
  averageNet?: number | null;
  monthsCount?: number | null;
}
