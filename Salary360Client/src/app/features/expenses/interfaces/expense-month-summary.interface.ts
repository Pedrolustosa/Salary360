import { ExpenseCategorySummary } from './expense-category-summary.interface';

export interface ExpenseMonthSummary {
  totalFixed: number;
  totalVariable: number;
  total: number;
  totalPaid: number;
  totalPending: number;
  count: number;
  byCategory: ExpenseCategorySummary[];
  hasSalary: boolean;
  netAmount: number | null;
  committedPercent: number | null;
  available: number | null;
}
