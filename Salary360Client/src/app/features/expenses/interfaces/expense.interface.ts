import { ExpenseType } from '../enums/expense-type.enum';

export interface Expense {
  id: string;
  categoryId: string;
  name: string;
  type: ExpenseType;
  amount: number;
  year: number;
  month: number;
  dueDay: number | null;
  isPaid: boolean;
  paidAtUtc: string | null;
  notes: string | null;
  seriesId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}
