import { SalaryItem } from './salary-item.interface';

export interface SalaryRecord {
  id: string;
  year: number;
  month: number;
  grossAmount: number;
  totalBenefits: number;
  totalDeductions: number;
  netAmount: number;
  notes: string | null;
  items: SalaryItem[];
  createdAtUtc: string;
  updatedAtUtc: string;
}
