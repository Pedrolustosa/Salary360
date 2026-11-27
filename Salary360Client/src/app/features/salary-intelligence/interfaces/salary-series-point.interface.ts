export interface SalarySeriesPoint {
  year: number;
  month: number;
  gross: number;
  net: number;
  benefits: number;
  deductions: number;
  grossChangeAbsolute: number | null;
  grossChangePercent: number | null;
  netChangeAbsolute: number | null;
  netChangePercent: number | null;
}
