import { SalaryEvolutionPoint } from '../../../core/salaries/interfaces/salary-evolution-point.interface';
import { EvolutionChartSeries } from '../interfaces/evolution-chart-series.interface';
import { EvolutionStats } from '../interfaces/evolution-stats.interface';

export function validateEvolutionRange(fromYear: number, toYear: number): string | null {
  if (!Number.isInteger(fromYear) || !Number.isInteger(toYear) || fromYear < 1900 || toYear > 2100) {
    return 'invalid_year';
  }
  if (fromYear > toYear) return 'invalid_year';
  if (toYear - fromYear > 100) return 'invalid_year';
  return null;
}

export function validateEvolutionMonth(month: number | null | undefined): string | null {
  if (month == null || month < 1 || month > 12) return 'invalid_month';
  return null;
}

export function toEvolutionChartData(points: SalaryEvolutionPoint[]): EvolutionChartSeries {
  return {
    labels: points.map((point) => String(point.year)),
    gross: points.map((point) => point.gross),
    net: points.map((point) => point.net),
    benefits: points.map((point) => point.benefits),
    deductions: points.map((point) => point.deductions),
  };
}

export function computeEvolutionStats(points: SalaryEvolutionPoint[]): EvolutionStats {
  const nets = points.map((point) => point.net).filter((value): value is number => value !== null);
  if (nets.length === 0) {
    return { maxNet: null, minNet: null, variationPercent: null };
  }

  const maxNet = Math.max(...nets);
  const minNet = Math.min(...nets);
  const first = nets[0];
  const last = nets[nets.length - 1];
  const variationPercent = first === 0 ? null : ((last - first) / Math.abs(first)) * 100;

  return { maxNet, minNet, variationPercent };
}
