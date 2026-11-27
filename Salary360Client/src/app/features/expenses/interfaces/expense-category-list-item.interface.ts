import { CategoryColor } from '../enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';

export interface ExpenseCategoryListItem {
  id: string;
  name: string | null;
  systemKey: string | null;
  colorKey: CategoryColor;
  iconKey: CategoryIcon;
  expenseCount: number;
  createdAtUtc: string;
  updatedAtUtc: string;
}
