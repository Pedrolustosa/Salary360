import {
  computeEvolutionStats,
  toEvolutionChartData,
  validateEvolutionMonth,
  validateEvolutionRange,
} from './evolution-chart';

describe('evolution-chart helpers', () => {
  it('should validate year ranges', () => {
    expect(validateEvolutionRange(2020, 2024)).toBeNull();
    expect(validateEvolutionRange(2025, 2020)).toBe('invalid_year');
    expect(validateEvolutionRange(1900, 2001)).toBe('invalid_year');
  });

  it('should validate months', () => {
    expect(validateEvolutionMonth(9)).toBeNull();
    expect(validateEvolutionMonth(0)).toBe('invalid_month');
    expect(validateEvolutionMonth(null)).toBe('invalid_month');
  });

  it('should keep null gaps in chart series', () => {
    const data = toEvolutionChartData([
      { year: 2020, gross: 1000, benefits: 100, deductions: 50, net: 1050 },
      { year: 2021, gross: null, benefits: null, deductions: null, net: null },
      { year: 2022, gross: 1100, benefits: 120, deductions: 40, net: 1180 },
    ]);

    expect(data.labels).toEqual(['2020', '2021', '2022']);
    expect(data.net).toEqual([1050, null, 1180]);
    expect(data.gross[1]).toBeNull();
  });

  it('should compute max, min and variation across non-null nets', () => {
    const stats = computeEvolutionStats([
      { year: 2020, gross: 1, benefits: 0, deductions: 0, net: 100 },
      { year: 2021, gross: null, benefits: null, deductions: null, net: null },
      { year: 2022, gross: 1, benefits: 0, deductions: 0, net: 150 },
    ]);

    expect(stats.maxNet).toBe(150);
    expect(stats.minNet).toBe(100);
    expect(stats.variationPercent).toBe(50);
  });

  it('should return null variation without a base', () => {
    expect(computeEvolutionStats([])).toEqual({ maxNet: null, minNet: null, variationPercent: null });
  });
});
