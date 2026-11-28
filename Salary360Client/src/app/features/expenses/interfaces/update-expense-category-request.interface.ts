import { CategoryColor } from '../../../shared/enums/category-color.enum';
import { CategoryIcon } from '../enums/category-icon.enum';

export interface UpdateExpenseCategoryRequest {
  name: string | null;
  colorKey: CategoryColor;
  iconKey: CategoryIcon;
}
