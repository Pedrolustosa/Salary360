import { ExpenseType } from '../enums/expense-type.enum';

export interface UpdateExpenseRequest {
  categoryId: string;
  name: string;
  type: ExpenseType;
  amount: number;
  year: number;
  month: number;
  dueDay: number | null;
  isPaid: boolean;
  notes: string | null;
}
