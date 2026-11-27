import {
  toEvolutionChartData,
  toProjectionChartData,
  toPurchasingPowerChartData,
} from './salary-intelligence-charts';

describe('salary-intelligence-charts', () => {
  it('should map evolution series with competence labels', () => {
    const data = toEvolutionChartData([
      {
        year: 2024,
        month: 1,
        gross: 1000,
        net: 800,
        benefits: 100,
        deductions: 50,
        grossChangeAbsolute: null,
        grossChangePercent: null,
        netChangeAbsolute: null,
        netChangePercent: null,
      },
      {
        year: 2024,
        month: 2,
        gross: 1100,
        net: 900,
        benefits: 100,
        deductions: 50,
        grossChangeAbsolute: 100,
        grossChangePercent: 10,
        netChangeAbsolute: 100,
        netChangePercent: 12.5,
      },
    ]);

    expect(data.labels).toEqual(['2024-01', '2024-02']);
    expect(data.gross).toEqual([1000, 1100]);
    expect(data.net).toEqual([800, 900]);
  });

  it('should keep null gaps in purchasing power series', () => {
    const data = toPurchasingPowerChartData([
      { year: 2024, month: 1, actual: 1000, inflationAdjusted: 1000 },
      { year: 2024, month: 2, actual: 1100, inflationAdjusted: null },
    ]);

    expect(data.inflationAdjusted).toEqual([1000, null]);
  });

  it('should keep null gaps in projection series', () => {
    const data = toProjectionChartData([
      { year: 2024, month: 1, actual: 1000, conservative: null, likely: null, optimistic: null },
      { year: 2024, month: 2, actual: null, conservative: 900, likely: 1000, optimistic: 1100 },
    ]);

    expect(data.actual).toEqual([1000, null]);
    expect(data.conservative).toEqual([1000, 900]);
    expect(data.likely).toEqual([1000, 1000]);
    expect(data.optimistic).toEqual([1000, 1100]);
  });

  it('should not anchor projection when there is no future point', () => {
    const data = toProjectionChartData([
      { year: 2024, month: 1, actual: 1000, conservative: null, likely: null, optimistic: null },
    ]);

    expect(data.actual).toEqual([1000]);
    expect(data.conservative).toEqual([null]);
    expect(data.likely).toEqual([null]);
    expect(data.optimistic).toEqual([null]);
  });
});
