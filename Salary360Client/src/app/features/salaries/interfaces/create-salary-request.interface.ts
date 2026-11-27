import { SalaryItemInput } from './salary-item-input.interface';

export interface CreateSalaryRequest {
  year: number;
  month: number;
  grossAmount: number;
  notes: string | null;
  items: SalaryItemInput[];
}
