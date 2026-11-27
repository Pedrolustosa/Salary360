import { SalaryItemType } from '../enums/salary-item-type.enum';
import { calculateSalarySummary } from './salary-summary';

describe('calculateSalarySummary', () => {
  it('should compute benefits, deductions and net with even rounding', () => {
    const summary = calculateSalarySummary(1000.005, [
      { type: SalaryItemType.Benefit, amount: 100.125 },
      { type: SalaryItemType.Deduction, amount: 50.125 },
      { type: SalaryItemType.Benefit, amount: 10 },
    ]);

    expect(summary.gross).toBe(1000);
    expect(summary.benefits).toBe(110.12);
    expect(summary.deductions).toBe(50.12);
    expect(summary.net).toBe(1060);
  });

  it('should allow a negative net', () => {
    const summary = calculateSalarySummary(100, [{ type: SalaryItemType.Deduction, amount: 250 }]);
    expect(summary.net).toBe(-150);
  });

  it('should handle an empty item list and null amounts', () => {
    expect(calculateSalarySummary(null, [])).toEqual({ gross: 0, benefits: 0, deductions: 0, net: 0 });
    expect(calculateSalarySummary(500, [{ type: SalaryItemType.Benefit, amount: null }])).toEqual({
      gross: 500,
      benefits: 0,
      deductions: 0,
      net: 500,
    });
  });

  it('should support up to 50 items', () => {
    const items = Array.from({ length: 50 }, () => ({ type: SalaryItemType.Benefit, amount: 1 }));
    expect(calculateSalarySummary(100, items).benefits).toBe(50);
  });
});
