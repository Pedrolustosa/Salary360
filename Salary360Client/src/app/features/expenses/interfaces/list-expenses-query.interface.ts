import { ExpenseType } from '../enums/expense-type.enum';

export interface ListExpensesQuery {
  year: number;
  month: number;
  categoryId?: string | null;
  type?: ExpenseType | null;
  isPaid?: boolean | null;
  page?: number;
  pageSize?: number;
}
