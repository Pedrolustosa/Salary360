import { SalaryItemType } from '../enums/salary-item-type.enum';

export interface SalaryItemInput {
  name: string;
  type: SalaryItemType;
  amount: number;
}
