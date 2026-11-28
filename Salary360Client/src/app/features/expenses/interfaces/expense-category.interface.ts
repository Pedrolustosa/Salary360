import { CategoryColor } from '../../../shared/enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';

export interface ExpenseCategory {
  id: string;
  name: string | null;
  systemKey: string | null;
  colorKey: CategoryColor;
  iconKey: CategoryIcon;
  createdAtUtc: string;
  updatedAtUtc: string;
}
