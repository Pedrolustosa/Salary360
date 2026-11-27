import { CategoryColor } from '../enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';

export interface CreateExpenseCategoryRequest {
  name: string;
  colorKey: CategoryColor;
  iconKey: CategoryIcon;
}
