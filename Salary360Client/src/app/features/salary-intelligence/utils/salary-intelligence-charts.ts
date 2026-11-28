import { EvolutionChartData } from '../interfaces/evolution-chart-data.interface';
import { ProjectionChartData } from '../interfaces/projection-chart-data.interface';
import { ProjectionPoint } from '../interfaces/projection-point.interface';
import { PurchasingPowerChartData } from '../interfaces/purchasing-power-chart-data.interface';
import { PurchasingPowerPoint } from '../interfaces/purchasing-power-point.interface';
import { SalarySeriesPoint } from '../interfaces/salary-series-point.interface';

export function competenceLabel(year: number, month: number): string {
  return `${year}-${String(month).padStart(2, '0')}`;
}

export function toEvolutionChartData(points: SalarySeriesPoint[]): EvolutionChartData {
  return {
    labels: points.map((point) => competenceLabel(point.year, point.month)),
    gross: points.map((point) => point.gross),
    net: points.map((point) => point.net),
  };
}

export function toPurchasingPowerChartData(points: PurchasingPowerPoint[]): PurchasingPowerChartData {
  return {
    labels: points.map((point) => competenceLabel(point.year, point.month)),
    actual: points.map((point) => point.actual),
    inflationAdjusted: points.map((point) => point.inflationAdjusted),
  };
}

export function toProjectionChartData(points: ProjectionPoint[]): ProjectionChartData {
  const actual = points.map((point) => point.actual);
  const conservative = points.map((point) => point.conservative);
  const likely = points.map((point) => point.likely);
  const optimistic = points.map((point) => point.optimistic);

  let lastActualIndex = -1;
  for (let index = actual.length - 1; index >= 0; index--) {
    if (actual[index] !== null) {
      lastActualIndex = index;
      break;
    }
  }

  if (lastActualIndex >= 0 && lastActualIndex + 1 < points.length) {
    const anchor = actual[lastActualIndex];
    if (anchor !== null) {
      if (conservative[lastActualIndex] === null) conservative[lastActualIndex] = anchor;
      if (likely[lastActualIndex] === null) likely[lastActualIndex] = anchor;
      if (optimistic[lastActualIndex] === null) optimistic[lastActualIndex] = anchor;
    }
  }

  return {
    labels: points.map((point) => competenceLabel(point.year, point.month)),
    actual,
    conservative,
    likely,
    optimistic,
  };
}
