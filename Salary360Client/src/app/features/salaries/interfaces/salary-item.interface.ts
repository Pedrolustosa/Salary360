import { SalaryItemType } from '../enums/salary-item-type.enum';

export interface SalaryItem {
  id: string;
  name: string;
  type: SalaryItemType;
  amount: number;
  order: number;
}
