import { roundToEven } from '../../../core/format/money';
import { SalaryItemType } from '../../../core/salaries/enums/salary-item-type.enum';
import { SalarySummary } from '../interfaces/salary-summary.interface';
import { SalarySummaryItem } from '../interfaces/salary-summary-item.interface';

export function calculateSalarySummary(gross: number | null, items: SalarySummaryItem[]): SalarySummary {
  const safeGross = roundToEven(gross ?? 0);
  const benefits = roundToEven(
    items
      .filter((item) => item.type === SalaryItemType.Benefit)
      .reduce((sum, item) => sum + (item.amount ?? 0), 0),
  );
  const deductions = roundToEven(
    items
      .filter((item) => item.type === SalaryItemType.Deduction)
      .reduce((sum, item) => sum + (item.amount ?? 0), 0),
  );
  return {
    gross: safeGross,
    benefits,
    deductions,
    net: roundToEven(safeGross + benefits - deductions),
  };
}
