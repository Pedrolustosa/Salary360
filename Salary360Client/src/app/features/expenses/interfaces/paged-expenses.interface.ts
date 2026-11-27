import { ExpenseListItem } from './expense-list-item.interface';

export interface PagedExpenses {
  items: ExpenseListItem[];
  total: number;
  page: number;
  pageSize: number;
}
