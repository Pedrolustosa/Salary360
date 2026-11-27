import { ExpenseRecurrence } from '../enums/expense-recurrence.enum';
import { ExpenseType } from '../enums/expense-type.enum';

export interface CreateExpenseRequest {
  categoryId: string;
  name: string;
  type: ExpenseType;
  amount: number;
  year: number;
  month: number;
  dueDay: number | null;
  isPaid: boolean;
  notes: string | null;
  recurrence: ExpenseRecurrence;
  recurrenceCount: number | null;
}
