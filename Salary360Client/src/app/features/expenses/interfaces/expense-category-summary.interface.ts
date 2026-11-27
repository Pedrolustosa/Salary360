import { CategoryColor } from '../enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';

export interface ExpenseCategorySummary {
  categoryId: string;
  name: string | null;
  systemKey: string | null;
  colorKey: CategoryColor;
  iconKey: CategoryIcon;
  total: number;
  percentOfTotal: number;
}
