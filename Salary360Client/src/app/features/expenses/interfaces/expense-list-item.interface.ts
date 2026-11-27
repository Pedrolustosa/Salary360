import { CategoryColor } from '../enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';
import { ExpenseType } from '../enums/expense-type.enum';

export interface ExpenseListItem {
  id: string;
  categoryId: string;
  categoryName: string | null;
  categorySystemKey: string | null;
  categoryColorKey: CategoryColor;
  categoryIconKey: CategoryIcon;
  name: string;
  type: ExpenseType;
  amount: number;
  year: number;
  month: number;
  dueDay: number | null;
  isPaid: boolean;
  paidAtUtc: string | null;
  seriesId: string | null;
}
