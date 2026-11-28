import { SalaryItemType } from '../../../core/salaries/enums/salary-item-type.enum';

export interface SalarySummaryItem {
  type: SalaryItemType | string;
  amount: number | null;
}
